using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Cryptography;
using Xunit;

namespace EquipmentBorrowing.Tests.Integration;

[Trait("Category", "A06Disposable")]
public sealed class PostgreSqlA06RollbackTests
{
    [Fact]
    public async Task MultiItemCheckout_WhenSecondInsertFails_RollsBackEverything()
    {
        string? connection = Environment.GetEnvironmentVariable(
            "EQUIPMENTBORROWING_A06_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException(
                "Set EQUIPMENTBORROWING_A06_CONNECTION_STRING for the disposable verification database.");

        var cs = new NpgsqlConnectionStringBuilder(connection);
        if (!string.Equals(cs.Database, "equipment_borrowing_lab4_verify",
                StringComparison.OrdinalIgnoreCase) ||
            !(string.Equals(cs.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
              cs.Host == "127.0.0.1"))
        {
            throw new InvalidOperationException(
                "A06 can run only against the LOCAL equipment_borrowing_lab4_verify database.");
        }

        var options = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>()
            .UseNpgsql(cs.ConnectionString).Options;
        var factory = new EquipmentBorrowingContextFactory(options);
        var service = new PostgreSqlAtomicBorrowingService(factory);

        int studentId = RandomNumberGenerator.GetInt32(100_000, 1_000_000_000);
        int firstId = RandomNumberGenerator.GetInt32(100_000, 1_000_000_000);
        int secondId = RandomNumberGenerator.GetInt32(100_000, 1_000_000_000);
        while (firstId == secondId || studentId == firstId || studentId == secondId)
            secondId = RandomNumberGenerator.GetInt32(100_000, 1_000_000_000);
        int[] ids = [firstId, secondId];

        // Unique, strictly alphanumeric object identifiers prevent name collisions.
        string suffix = Guid.NewGuid().ToString("N");
        string functionName = $"a06_fail_second_insert_{suffix}";
        string triggerName = $"a06_trigger_{suffix}";
        bool fixtureCreated = false;
        bool triggerCreated = false;
        bool functionCreated = false;

        try
        {
            await using (var db = await factory.CreateDbContextAsync())
            {
                db.Students.Add(new Student(studentId,
                    $"A06-STU-{studentId}", "A06 Rollback Test Student"));
                db.Equipment.Add(new Equipment(firstId,
                    $"A06-EQ-{firstId}", "A06 Rollback Equipment One"));
                db.Equipment.Add(new Equipment(secondId,
                    $"A06-EQ-{secondId}", "A06 Rollback Equipment Two"));
                await db.SaveChangesAsync();
                fixtureCreated = true;
            }

            // The trigger fires only for this fixture's student and only AFTER
            // the second borrowing row has been inserted within the transaction.
            await using (var conn = new NpgsqlConnection(cs.ConnectionString))
            {
                await conn.OpenAsync();
                await using (var makeFunction = new NpgsqlCommand($$"""
                    CREATE FUNCTION public."{{functionName}}"()
                    RETURNS trigger LANGUAGE plpgsql AS $trigger$
                    BEGIN
                        IF (SELECT COUNT(*) FROM public."Borrowings"
                            WHERE "StudentId" = NEW."StudentId") >= 2 THEN
                            RAISE EXCEPTION 'A06 controlled failure after second borrowing INSERT'
                                USING ERRCODE = 'P0001';
                        END IF;
                        RETURN NEW;
                    END;
                    $trigger$;
                    """, conn))
                {
                    await makeFunction.ExecuteNonQueryAsync();
                    functionCreated = true;
                }

                await using (var makeTrigger = new NpgsqlCommand($$"""
                    CREATE TRIGGER "{{triggerName}}"
                    AFTER INSERT ON public."Borrowings"
                    FOR EACH ROW WHEN (NEW."StudentId" = {{studentId}})
                    EXECUTE FUNCTION public."{{functionName}}"();
                    """, conn))
                {
                    await makeTrigger.ExecuteNonQueryAsync();
                    triggerCreated = true;
                }
            }

            var failed = await service.CheckoutAsync(
                studentId, ids, new DateOnly(2026, 11, 20),
                new DateOnly(2026, 11, 22));
            Assert.Equal(BorrowingOperationStatus.Failed, failed.Status);

            // This is a NEW context/connection: observe committed database state,
            // not EF's in-memory tracked changes.
            await using (var verifyDb = await factory.CreateDbContextAsync())
            {
                Assert.Equal(0, await verifyDb.Borrowings.AsNoTracking()
                    .CountAsync(b => b.StudentId == studentId));
                var availability = await verifyDb.Equipment.AsNoTracking()
                    .Where(e => ids.Contains(e.EquipmentId))
                    .Select(e => e.IsAvailable).ToListAsync();
                Assert.Equal(2, availability.Count);
                Assert.All(availability, value => Assert.True(value));
            }

            // Remove failure mechanism before verifying service recovery.
            await DropTestObjectsAsync(cs.ConnectionString, triggerName,
                functionName, triggerCreated, functionCreated);
            triggerCreated = false;
            functionCreated = false;

            var recovered = await service.CheckoutAsync(
                studentId, ids, new DateOnly(2026, 11, 20),
                new DateOnly(2026, 11, 22));
            Assert.Equal(BorrowingOperationStatus.Success, recovered.Status);

            await using (var verifyDb = await factory.CreateDbContextAsync())
            {
                Assert.Equal(2, await verifyDb.Borrowings.AsNoTracking()
                    .CountAsync(b => b.StudentId == studentId));
                var availability = await verifyDb.Equipment.AsNoTracking()
                    .Where(e => ids.Contains(e.EquipmentId))
                    .Select(e => e.IsAvailable).ToListAsync();
                Assert.Equal(2, availability.Count);
                Assert.All(availability, value => Assert.False(value));
            }
        }
        finally
        {
            // Cleanup runs even when assertions fail. If cleanup fails, do not
            // reuse the disposable DB until the leftover trigger is inspected.
            await DropTestObjectsAsync(cs.ConnectionString, triggerName,
                functionName, triggerCreated, functionCreated);
            if (fixtureCreated)
            {
                await using var db = await factory.CreateDbContextAsync();
                await db.Borrowings.Where(b => b.StudentId == studentId)
                    .ExecuteDeleteAsync();
                await db.Equipment.Where(e => ids.Contains(e.EquipmentId))
                    .ExecuteDeleteAsync();
                await db.Students.Where(s => s.StudentId == studentId)
                    .ExecuteDeleteAsync();
            }
        }
    }

    private static async Task DropTestObjectsAsync(string connection,
        string triggerName, string functionName, bool dropTrigger,
        bool dropFunction)
    {
        await using var conn = new NpgsqlConnection(connection);
        await conn.OpenAsync();
        if (dropTrigger)
        {
            await using var cmd = new NpgsqlCommand(
                $"DROP TRIGGER IF EXISTS \"{triggerName}\" ON public.\"Borrowings\";", conn);
            await cmd.ExecuteNonQueryAsync();
        }
        if (dropFunction)
        {
            await using var cmd = new NpgsqlCommand(
                $"DROP FUNCTION IF EXISTS public.\"{functionName}\"();", conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}

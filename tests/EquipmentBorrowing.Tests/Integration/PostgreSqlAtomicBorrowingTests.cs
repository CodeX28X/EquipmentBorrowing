
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Cryptography;
using Xunit;

namespace EquipmentBorrowing.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class PostgreSqlAtomicBorrowingTests
{
    private static string GetConnectionString()
    {
        string? connection = Environment.GetEnvironmentVariable(
            "EQUIPMENTBORROWING_TEST_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException(
                "Set EQUIPMENTBORROWING_TEST_CONNECTION_STRING.");

        var builder = new NpgsqlConnectionStringBuilder(connection);

        if (!string.Equals(
                builder.Database,
                "equipment_borrowing_test",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Integration tests must use equipment_borrowing_test.");
        }

        return connection;
    }

    private static (
        EquipmentBorrowingContextFactory Factory,
        PostgreSqlAtomicBorrowingService Service)
        CreateServices()
    {
        var options =
            new DbContextOptionsBuilder<EquipmentBorrowingDbContext>()
                .UseNpgsql(GetConnectionString())
                .Options;

        var factory =
            new EquipmentBorrowingContextFactory(options);

        return (
            factory,
            new PostgreSqlAtomicBorrowingService(factory)
        );
    }

    private static async Task<(int StudentId, int[] EquipmentIds)>
        SeedAsync(
            EquipmentBorrowingContextFactory factory,
            int equipmentCount)
    {
        // Distinct positive IDs allow independent test records.
        int studentId =
            RandomNumberGenerator.GetInt32(100000, 2000000000);

        int[] equipmentIds =
            Enumerable.Range(0, equipmentCount)
                .Select(_ => RandomNumberGenerator.GetInt32(
                    100000, 2000000000))
                .Distinct()
                .ToArray();

        while (equipmentIds.Length < equipmentCount)
        {
            equipmentIds = equipmentIds
                .Append(RandomNumberGenerator.GetInt32(
                    100000, 2000000000))
                .Distinct()
                .ToArray();
        }

        await using var db =
            await factory.CreateDbContextAsync();

        db.Students.Add(new Student(
            studentId,
            $"IT-{studentId}",
            "Integration Test Student"));

        foreach (int id in equipmentIds)
        {
            db.Equipment.Add(new Equipment(
                id,
                $"IT-EQ-{id}",
                $"Integration Test Equipment {id}"));
        }

        await db.SaveChangesAsync();

        return (studentId, equipmentIds);
    }

    private static async Task CleanupAsync(
        EquipmentBorrowingContextFactory factory,
        int studentId,
        int[] equipmentIds)
    {
        await using var db =
            await factory.CreateDbContextAsync();

        await db.Borrowings
            .Where(b => b.StudentId == studentId)
            .ExecuteDeleteAsync();

        await db.Equipment
            .Where(e => equipmentIds.Contains(e.EquipmentId))
            .ExecuteDeleteAsync();

        await db.Students
            .Where(s => s.StudentId == studentId)
            .ExecuteDeleteAsync();
    }

    private static readonly DateOnly CheckoutDate =
        new(2026, 11, 18);

    private static readonly DateOnly DueDate =
        new(2026, 11, 20);

    [Fact]
    public async Task CheckoutAndReturn_PersistCorrectly()
    {
        var (factory, service) = CreateServices();
        var (studentId, items) = await SeedAsync(factory, 1);

        try
        {
            var checkout = await service.CheckoutAsync(
                studentId,
                items,
                CheckoutDate,
                DueDate);

            Assert.Equal(
                BorrowingOperationStatus.Success,
                checkout.Status);

            await using (var db =
                await factory.CreateDbContextAsync())
            {
                var borrowing = await db.Borrowings
                    .SingleAsync(b => b.StudentId == studentId);

                var equipment = await db.Equipment
                    .SingleAsync(e => e.EquipmentId == items[0]);

                Assert.Equal(BorrowingStatus.Active, borrowing.Status);
                Assert.False(equipment.IsAvailable);
            }

            var returned = await service.ReturnAsync(
                studentId,
                items[0],
                CheckoutDate);

            Assert.Equal(
                BorrowingOperationStatus.Success,
                returned.Status);

            await using (var db =
                await factory.CreateDbContextAsync())
            {
                var borrowing = await db.Borrowings
                    .SingleAsync(b => b.StudentId == studentId);

                var equipment = await db.Equipment
                    .SingleAsync(e => e.EquipmentId == items[0]);

                Assert.Equal(BorrowingStatus.Returned, borrowing.Status);
                Assert.Equal(CheckoutDate, borrowing.ActualReturnDate);
                Assert.True(equipment.IsAvailable);
            }
        }
        finally
        {
            await CleanupAsync(factory, studentId, items);
        }
    }

    [Fact]
    public async Task MultiItemCheckout_IsAllOrNothing()
    {
        var (factory, service) = CreateServices();
        var (studentId, items) = await SeedAsync(factory, 3);

        try
        {
            var initial = await service.CheckoutAsync(
                studentId,
                [items[0]],
                CheckoutDate,
                DueDate);

            Assert.Equal(
                BorrowingOperationStatus.Success,
                initial.Status);

            var rejected = await service.CheckoutAsync(
                studentId,
                [items[1], items[0]],
                CheckoutDate,
                DueDate);

            Assert.Equal(
                BorrowingOperationStatus.Rejected,
                rejected.Status);

            await using var db =
                await factory.CreateDbContextAsync();

            int activeCount = await db.Borrowings
                .CountAsync(b => b.StudentId == studentId &&
                    b.Status == BorrowingStatus.Active);

            bool otherItemAvailable = await db.Equipment
                .Where(e => e.EquipmentId == items[1])
                .Select(e => e.IsAvailable)
                .SingleAsync();

            Assert.Equal(1, activeCount);
            Assert.True(otherItemAvailable);
        }
        finally
        {
            await CleanupAsync(factory, studentId, items);
        }
    }

    [Fact]
    public async Task FourthActiveItem_IsRejected()
    {
        var (factory, service) = CreateServices();
        var (studentId, items) = await SeedAsync(factory, 4);

        try
        {
            var first = await service.CheckoutAsync(
                studentId,
                items.Take(3).ToArray(),
                CheckoutDate,
                DueDate);

            Assert.Equal(
                BorrowingOperationStatus.Success,
                first.Status);

            var fourth = await service.CheckoutAsync(
                studentId,
                [items[3]],
                CheckoutDate,
                DueDate);

            Assert.Equal(
                BorrowingOperationStatus.Rejected,
                fourth.Status);

            await using var db =
                await factory.CreateDbContextAsync();

            int activeCount = await db.Borrowings
                .CountAsync(b => b.StudentId == studentId &&
                    b.Status == BorrowingStatus.Active);

            Assert.Equal(3, activeCount);
        }
        finally
        {
            await CleanupAsync(factory, studentId, items);
        }
    }

    [Fact]
    public async Task ConcurrentCheckouts_CannotExceedStudentLimit()
    {
        var (factory, service) = CreateServices();
        var (studentId, items) = await SeedAsync(factory, 4);

        try
        {
            var results = await Task.WhenAll(
                service.CheckoutAsync(
                    studentId,
                    [items[0], items[1]],
                    CheckoutDate,
                    DueDate),
                service.CheckoutAsync(
                    studentId,
                    [items[2], items[3]],
                    CheckoutDate,
                    DueDate));


            Assert.Single(
                results,
                r => r.Status == BorrowingOperationStatus.Success);

            Assert.Single(
                results,
                r => r.Status == BorrowingOperationStatus.Rejected);


            await using var db =
                await factory.CreateDbContextAsync();

            int activeCount = await db.Borrowings
                .CountAsync(b => b.StudentId == studentId &&
                    b.Status == BorrowingStatus.Active);

            Assert.Equal(2, activeCount);
        }
        finally
        {
            await CleanupAsync(factory, studentId, items);
        }
    }


    [Fact]
    public async Task MultiItemCheckout_PersistsAllSelectedItems()
    {
        var (factory, service) = CreateServices();
        var (studentId, items) = await SeedAsync(factory, 3);

        try
        {
            var result = await service.CheckoutAsync(
                studentId,
                items,
                CheckoutDate,
                DueDate);

            Assert.Equal(
                BorrowingOperationStatus.Success,
                result.Status);

            Assert.Equal(3, result.BorrowingIds.Count);

            await using var db =
                await factory.CreateDbContextAsync();

            var borrowings = await db.Borrowings
                .AsNoTracking()
                .Where(b => b.StudentId == studentId)
                .ToListAsync();

            Assert.Equal(3, borrowings.Count);

            Assert.All(borrowings, borrowing =>
            {
                Assert.Equal(
                    BorrowingStatus.Active,
                    borrowing.Status);

                Assert.Equal(DueDate, borrowing.ExpectedReturnDate);
            });

            var equipment = await db.Equipment
                .AsNoTracking()
                .Where(e => items.Contains(e.EquipmentId))
                .ToListAsync();

            Assert.Equal(3, equipment.Count);
            Assert.All(equipment, e => Assert.False(e.IsAvailable));
        }
        finally
        {
            await CleanupAsync(factory, studentId, items);
        }
    }


    [Fact]
    public async Task ReturningSameBorrowingTwice_IsRejected()
    {
        var (factory, service) = CreateServices();
        var (studentId, items) = await SeedAsync(factory, 1);

        try
        {
            var checkout = await service.CheckoutAsync(
                studentId,
                items,
                CheckoutDate,
                DueDate);

            Assert.Equal(
                BorrowingOperationStatus.Success,
                checkout.Status);

            var firstReturn = await service.ReturnAsync(
                studentId,
                items[0],
                CheckoutDate);

            Assert.Equal(
                BorrowingOperationStatus.Success,
                firstReturn.Status);

            var secondReturn = await service.ReturnAsync(
                studentId,
                items[0],
                CheckoutDate);

            Assert.Equal(
                BorrowingOperationStatus.Rejected,
                secondReturn.Status);

            await using var db =
                await factory.CreateDbContextAsync();

            var borrowing = await db.Borrowings
                .AsNoTracking()
                .SingleAsync(b => b.StudentId == studentId);

            var equipment = await db.Equipment
                .AsNoTracking()
                .SingleAsync(e => e.EquipmentId == items[0]);

            Assert.Equal(
                BorrowingStatus.Returned,
                borrowing.Status);

            Assert.Equal(
                CheckoutDate,
                borrowing.ActualReturnDate);

            Assert.True(equipment.IsAvailable);
        }
        finally
        {
            await CleanupAsync(factory, studentId, items);
        }
    }


}

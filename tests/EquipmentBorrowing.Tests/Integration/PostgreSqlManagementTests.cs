using EquipmentBorrowing.Application;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace EquipmentBorrowing.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class PostgreSqlManagementTests
{
    private static EquipmentBorrowingContextFactory CreateFactory()
    {
        string? connection = Environment.GetEnvironmentVariable(
            "EQUIPMENTBORROWING_TEST_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException(
                "Set EQUIPMENTBORROWING_TEST_CONNECTION_STRING.");

        var builder = new NpgsqlConnectionStringBuilder(connection);
        if (!string.Equals(builder.Database, "equipment_borrowing_test",
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Integration tests must use equipment_borrowing_test.");

        var options = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>()
            .UseNpgsql(connection)
            .Options;
        return new EquipmentBorrowingContextFactory(options);
    }

    private static string Unique(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}";

    private static async Task DeleteEquipmentAsync(
        EquipmentBorrowingContextFactory factory, int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.Equipment.Where(x => x.EquipmentId == id)
            .ExecuteDeleteAsync();
    }

    private static async Task DeleteStudentAsync(
        EquipmentBorrowingContextFactory factory, int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.Students.Where(x => x.StudentId == id)
            .ExecuteDeleteAsync();
    }

    [Fact]
    public async Task EquipmentRegistration_PersistsNormalizedFields_AndAvailability()
    {
        var factory = CreateFactory();
        var service = new PostgreSqlEquipmentManagementService(factory);
        string identifier = Unique("QA-EQ");
        int? id = null;
        try
        {
            var result = await service.RegisterAsync($"  {identifier}  ",
                "  QA Equipment  ", "  Description  ");
            Assert.Equal(EquipmentOperationStatus.Success, result.Status);
            id = Assert.IsType<int>(result.EquipmentId);

            await using var db = await factory.CreateDbContextAsync();
            var item = await db.Equipment.AsNoTracking()
                .SingleAsync(e => e.EquipmentId == id.Value);
            Assert.Equal(identifier, item.EquipmentIdentifier);
            Assert.Equal("QA Equipment", item.EquipmentName);
            Assert.Equal("Description", item.Description);
            Assert.True(item.IsAvailable);
        }
        finally { if (id.HasValue) await DeleteEquipmentAsync(factory, id.Value); }
    }

    [Fact]
    public async Task EquipmentRegistration_DuplicateCaseInsensitive_IsRejected()
    {
        var factory = CreateFactory();
        var service = new PostgreSqlEquipmentManagementService(factory);
        string identifier = Unique("QA-DUP");
        int? id = null;
        try
        {
            var first = await service.RegisterAsync(identifier, "Test Item", null);
            Assert.Equal(EquipmentOperationStatus.Success, first.Status);
            id = Assert.IsType<int>(first.EquipmentId);

            var duplicate = await service.RegisterAsync(
                $"  {identifier.ToLowerInvariant()}  ", "Other Item", null);
            Assert.Equal(EquipmentOperationStatus.Rejected, duplicate.Status);

            await using var db = await factory.CreateDbContextAsync();
            Assert.Equal(1, await db.Equipment.CountAsync(
                e => e.EquipmentId == id.Value));
        }
        finally { if (id.HasValue) await DeleteEquipmentAsync(factory, id.Value); }
    }

    [Fact]
    public async Task EquipmentEdit_PreservesIdAndAvailability()
    {
        var factory = CreateFactory();
        var service = new PostgreSqlEquipmentManagementService(factory);
        string identifier = Unique("QA-EDIT");
        int? id = null;
        try
        {
            var registered = await service.RegisterAsync(identifier, "Before", null);
            Assert.Equal(EquipmentOperationStatus.Success, registered.Status);
            id = Assert.IsType<int>(registered.EquipmentId);

            await using (var db = await factory.CreateDbContextAsync())
            {
                var item = await db.Equipment.SingleAsync(x => x.EquipmentId == id.Value);
                item.MarkAsBorrowed();
                await db.SaveChangesAsync();
            }

            var edited = await service.UpdateAsync(id.Value, identifier,
                "After", "  Updated details  ");
            Assert.Equal(EquipmentOperationStatus.Success, edited.Status);

            await using var verify = await factory.CreateDbContextAsync();
            var updated = await verify.Equipment.AsNoTracking()
                .SingleAsync(x => x.EquipmentId == id.Value);
            Assert.Equal(id.Value, updated.EquipmentId);
            Assert.Equal("After", updated.EquipmentName);
            Assert.Equal("Updated details", updated.Description);
            Assert.False(updated.IsAvailable);
        }
        finally { if (id.HasValue) await DeleteEquipmentAsync(factory, id.Value); }
    }

    [Fact]
    public async Task EquipmentEdit_MissingRecord_IsRejected()
    {
        var factory = CreateFactory();
        var service = new PostgreSqlEquipmentManagementService(factory);
        var result = await service.UpdateAsync(int.MaxValue,
            Unique("QA-MISSING"), "Missing", null);
        Assert.Equal(EquipmentOperationStatus.Rejected, result.Status);
    }

    [Fact]
    public async Task EquipmentRegistration_RequiredFields_AreRejected()
    {
        var service = new PostgreSqlEquipmentManagementService(CreateFactory());
        Assert.Equal(EquipmentOperationStatus.Rejected,
            (await service.RegisterAsync("   ", "Name", null)).Status);
        Assert.Equal(EquipmentOperationStatus.Rejected,
            (await service.RegisterAsync(Unique("QA"), "  ", null)).Status);
    }

    [Fact]
    public async Task StudentRegistration_PersistsYear_AndDefaultsEligible()
    {
        var factory = CreateFactory();
        var service = new PostgreSqlStudentManagementService(factory);
        int? id = null;
        try
        {
            var result = await service.RegisterAsync(Unique("QA-ST"),
                "  QA Student  ", "   ", 2);
            Assert.Equal(StudentOperationStatus.Success, result.Status);
            id = Assert.IsType<int>(result.StudentId);

            await using var db = await factory.CreateDbContextAsync();
            var student = await db.Students.AsNoTracking()
                .SingleAsync(s => s.StudentId == id.Value);
            Assert.Equal("QA Student", student.StudentName);
            Assert.Null(student.ContactInformation);
            Assert.Equal(2, student.StudentYear);
            Assert.True(student.IsAllowedToBorrow);
        }
        finally { if (id.HasValue) await DeleteStudentAsync(factory, id.Value); }
    }

    [Fact]
    public async Task StudentEdit_UpdatesYearWithoutResettingEligibility()
    {
        var factory = CreateFactory();
        var service = new PostgreSqlStudentManagementService(factory);
        string number = Unique("QA-YEAR");
        int? id = null;
        try
        {
            var registered = await service.RegisterAsync(number, "Before", null, 1);
            Assert.Equal(StudentOperationStatus.Success, registered.Status);
            id = Assert.IsType<int>(registered.StudentId);

            var eligibility = await service.SetEligibilityAsync(id.Value, false);
            Assert.Equal(StudentOperationStatus.Success, eligibility.Status);

            var edited = await service.UpdateAsync(id.Value, number,
                "After", "  Contact  ", 4);
            Assert.Equal(StudentOperationStatus.Success, edited.Status);

            await using var db = await factory.CreateDbContextAsync();
            var student = await db.Students.AsNoTracking()
                .SingleAsync(s => s.StudentId == id.Value);
            Assert.Equal("After", student.StudentName);
            Assert.Equal("Contact", student.ContactInformation);
            Assert.Equal(4, student.StudentYear);
            Assert.False(student.IsAllowedToBorrow);
        }
        finally { if (id.HasValue) await DeleteStudentAsync(factory, id.Value); }
    }

    [Fact]
    public async Task StudentRegistration_DuplicateNumber_IsRejected()
    {
        var factory = CreateFactory();
        var service = new PostgreSqlStudentManagementService(factory);
        string number = Unique("QA-ST-DUP");
        int? id = null;
        try
        {
            var first = await service.RegisterAsync(number, "Original", null, null);
            Assert.Equal(StudentOperationStatus.Success, first.Status);
            id = Assert.IsType<int>(first.StudentId);

            var duplicate = await service.RegisterAsync(
                $" {number.ToLowerInvariant()} ", "Duplicate", null, null);
            Assert.Equal(StudentOperationStatus.Rejected, duplicate.Status);
        }
        finally { if (id.HasValue) await DeleteStudentAsync(factory, id.Value); }
    }

    [Fact]
    public async Task StudentEdit_MissingRecord_IsRejected()
    {
        var service = new PostgreSqlStudentManagementService(CreateFactory());
        var result = await service.UpdateAsync(int.MaxValue,
            Unique("QA-NONE"), "Missing", null, null);
        Assert.Equal(StudentOperationStatus.Rejected, result.Status);
    }
}


using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests.Application;

public class BorrowEquipmentServiceTest
{
    private static readonly DateTime Checkout =
        new(2025, 1, 10);

    private static readonly DateTime Due =
        new(2025, 1, 17);

    private static Student Student(int id = 1) =>
        new(id, $"STU-{id:000}", "John Doe");

    private static Equipment Equipment(int id = 1) =>
        new(id, $"EQ-{id:000}", $"Equipment {id}");

    private static BorrowEquipmentService CreateService(
        IEnumerable<Student> students,
        IEnumerable<Equipment> items,
        InMemoryBorrowingRepository borrowings)
    {
        return new BorrowEquipmentService(
            new InMemoryStudentRepository(students),
            new InMemoryEquipmentRepository(items),
            borrowings,
            maximumActiveBorrowings: 3);
    }

    [Fact]
    public async Task ValidBorrow_CreatesActiveRecord()
    {
        var student = Student();
        var item = Equipment();
        var repo = new InMemoryBorrowingRepository();
        var service = CreateService(
            new[] { student }, new[] { item }, repo);

        var result = await service.BorrowAsync(
            1, 1, Checkout, Due);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Borrowing);
        Assert.Equal(BorrowingStatus.Active,
            result.Borrowing.Status);
        Assert.Equal(new DateOnly(2025, 1, 10),
            result.Borrowing.BorrowedAt);
        Assert.False(item.IsAvailable);
        Assert.Equal(1,
            await repo.CountActiveByStudentAsync(1));
    }

    [Fact]
    public async Task MissingStudent_IsRejected()
    {
        var item = Equipment();
        var repo = new InMemoryBorrowingRepository();
        var service = CreateService(
            Array.Empty<Student>(), new[] { item }, repo);

        var result = await service.BorrowAsync(
            999, 1, Checkout, Due);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Borrowing);
        Assert.True(item.IsAvailable);
        Assert.Empty(await repo.GetActiveAsync());
    }

    [Fact]
    public async Task RestrictedStudent_IsRejected()
    {
        var student = Student();
        student.SetBorrowingEligibility(false);
        var item = Equipment();
        var repo = new InMemoryBorrowingRepository();
        var service = CreateService(
            new[] { student }, new[] { item }, repo);

        var result = await service.BorrowAsync(
            1, 1, Checkout, Due);

        Assert.False(result.IsSuccess);
        Assert.True(item.IsAvailable);
        Assert.Empty(await repo.GetActiveAsync());
    }

    [Fact]
    public async Task ThreeActiveBorrowings_BlockFourth()
    {
        var student = Student();
        var existing = Enumerable.Range(1, 3)
            .Select(id => new Borrowing(
                student,
                Equipment(id),
                new DateOnly(2025, 1, 1),
                new DateOnly(2025, 1, 8)))
            .ToArray();

        var next = Equipment(4);
        var repo = new InMemoryBorrowingRepository(existing);
        var service = CreateService(
            new[] { student }, new[] { next }, repo);

        var result = await service.BorrowAsync(
            1, 4, Checkout, Due);

        Assert.False(result.IsSuccess);
        Assert.True(next.IsAvailable);
        Assert.Equal(3,
            await repo.CountActiveByStudentAsync(1));
    }

    [Fact]
    public async Task MissingEquipment_IsRejected()
    {
        var repo = new InMemoryBorrowingRepository();
        var service = CreateService(
            new[] { Student() },
            Array.Empty<Equipment>(), repo);

        var result = await service.BorrowAsync(
            1, 999, Checkout, Due);

        Assert.False(result.IsSuccess);
        Assert.Empty(await repo.GetActiveAsync());
    }

    [Fact]
    public async Task UnavailableEquipment_IsRejected()
    {
        var item = Equipment();
        item.MarkAsBorrowed();
        var repo = new InMemoryBorrowingRepository();
        var service = CreateService(
            new[] { Student() }, new[] { item }, repo);

        var result = await service.BorrowAsync(
            1, 1, Checkout, Due);

        Assert.False(result.IsSuccess);
        Assert.False(item.IsAvailable);
        Assert.Empty(await repo.GetActiveAsync());
    }

    [Theory]
    [InlineData(2025, 1, 9)]
    [InlineData(2025, 1, 10)]
    public async Task InvalidExpectedDate_IsRejected(
        int year, int month, int day)
    {
        var item = Equipment();
        var repo = new InMemoryBorrowingRepository();
        var service = CreateService(
            new[] { Student() }, new[] { item }, repo);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.BorrowAsync(
                1, 1, Checkout,
                new DateTime(year, month, day)));

        Assert.True(item.IsAvailable);
        Assert.Empty(await repo.GetActiveAsync());
    }

    [Fact]
    public void InvalidBorrowingLimit_IsRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new BorrowEquipmentService(
                new InMemoryStudentRepository(),
                new InMemoryEquipmentRepository(),
                new InMemoryBorrowingRepository(),
                maximumActiveBorrowings: 0));
    }
}

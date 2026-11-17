
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests.Infrastructure.Repositories;

public class InMemoryBorrowingRepositoryTests
{
    private static Student CreateStudent(int id = 1) =>
        new(id, $"STU-{id:000}", $"Student {id}");

    private static Equipment CreateEquipment(int id) =>
        new(id, $"EQ-{id:000}", $"Equipment {id}");

    private static Borrowing CreateBorrowing(
        Student student, Equipment equipment) =>
        new(
            student,
            equipment,
            new DateOnly(2025, 1, 10),
            new DateOnly(2025, 1, 17));

    [Fact]
    public async Task NoBorrowings_ActiveCountIsZero()
    {
        var repo = new InMemoryBorrowingRepository();

        Assert.Equal(0,
            await repo.CountActiveByStudentAsync(1));
    }

    [Fact]
    public async Task ActiveBorrowings_AreCounted()
    {
        var student = CreateStudent();
        var first = CreateBorrowing(
            student, CreateEquipment(1));
        var second = CreateBorrowing(
            student, CreateEquipment(2));

        var repo = new InMemoryBorrowingRepository(
            new[] { first, second });

        Assert.Equal(2,
            await repo.CountActiveByStudentAsync(1));
    }

    [Fact]
    public async Task ReturnedBorrowing_IsNotCounted()
    {
        var student = CreateStudent();
        var borrowing = CreateBorrowing(
            student, CreateEquipment(1));

        borrowing.MarkAsReturned(
            new DateOnly(2025, 1, 12));

        var repo = new InMemoryBorrowingRepository(
            new[] { borrowing });

        Assert.Equal(0,
            await repo.CountActiveByStudentAsync(1));
    }

    [Fact]
    public async Task AddAsync_StoresNewBorrowing()
    {
        var student = CreateStudent();
        var borrowing = CreateBorrowing(
            student, CreateEquipment(1));

        var repo = new InMemoryBorrowingRepository();

        await repo.AddAsync(borrowing);

        Assert.Equal(1,
            await repo.CountActiveByStudentAsync(1));
    }

    [Fact]
    public async Task GetActive_ExcludesReturnedRecords()
    {
        var student = CreateStudent();

        var active = CreateBorrowing(
            student, CreateEquipment(1));

        var returned = CreateBorrowing(
            student, CreateEquipment(2));

        returned.MarkAsReturned(
            new DateOnly(2025, 1, 12));

        var repo = new InMemoryBorrowingRepository(
            new[] { active, returned });

        var results = await repo.GetActiveAsync();

        Assert.Single(results);
        Assert.Contains(active, results);
        Assert.DoesNotContain(returned, results);
    }

    [Fact]
    public async Task FindActiveByStudentAndEquipment_ReturnsMatch()
    {
        var student = CreateStudent();
        var equipment = CreateEquipment(1);
        var borrowing = CreateBorrowing(
            student, equipment);

        var repo = new InMemoryBorrowingRepository(
            new[] { borrowing });

        var result =
            await repo.GetActiveByStudentAndEquipmentAsync(
                student.StudentId,
                equipment.EquipmentId);

        Assert.Same(borrowing, result);
    }

    [Fact]
    public async Task FindActive_DoesNotReturnCompletedLoan()
    {
        var student = CreateStudent();
        var equipment = CreateEquipment(1);
        var borrowing = CreateBorrowing(
            student, equipment);

        borrowing.MarkAsReturned(
            new DateOnly(2025, 1, 12));

        var repo = new InMemoryBorrowingRepository(
            new[] { borrowing });

        var result =
            await repo.GetActiveByStudentAndEquipmentAsync(
                student.StudentId,
                equipment.EquipmentId);

        Assert.Null(result);
        Assert.Equal(BorrowingStatus.Returned,
            borrowing.Status);
    }

    [Fact]
    public async Task DifferentStudent_HasSeparateCount()
    {
        var first = CreateBorrowing(
            CreateStudent(1), CreateEquipment(1));

        var second = CreateBorrowing(
            CreateStudent(2), CreateEquipment(2));

        var repo = new InMemoryBorrowingRepository(
            new[] { first, second });

        Assert.Equal(1,
            await repo.CountActiveByStudentAsync(1));

        Assert.Equal(1,
            await repo.CountActiveByStudentAsync(2));
    }
}

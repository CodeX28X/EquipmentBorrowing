
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;

namespace EquipmentBorrowing.Tests.Domain;

public class BorrowingTests
{
    private static Student CreateStudent() =>
        new Student(1, "STU-001", "Juan Dela Cruz");

    private static Equipment CreateEquipment() =>
        new Equipment(1, "LAP-001", "Laptop");

    private static Borrowing CreateBorrowing() =>
        new Borrowing(
            CreateStudent(),
            CreateEquipment(),
            new DateOnly(2026, 11, 18),
            new DateOnly(2026, 11, 25));

    [Fact]
    public void ValidBorrowing_IsInitiallyActive()
    {
        var student = CreateStudent();
        var equipment = CreateEquipment();

        var borrowing = new Borrowing(
            student,
            equipment,
            new DateOnly(2026, 11, 18),
            new DateOnly(2026, 11, 25));

        Assert.Equal(student.StudentId, borrowing.StudentId);
        Assert.Equal(equipment.EquipmentId, borrowing.EquipmentId);
        Assert.Same(student, borrowing.Student);
        Assert.Same(equipment, borrowing.Equipment);
        Assert.Equal(BorrowingStatus.Active, borrowing.Status);
        Assert.Null(borrowing.ActualReturnDate);
    }

    [Fact]
    public void MissingStudent_IsRejected()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Borrowing(
                null!,
                CreateEquipment(),
                new DateOnly(2026, 11, 18),
                new DateOnly(2026, 11, 25)));
    }

    [Fact]
    public void MissingEquipment_IsRejected()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Borrowing(
                CreateStudent(),
                null!,
                new DateOnly(2026, 11, 18),
                new DateOnly(2026, 11, 25)));
    }

    [Fact]
    public void EarlierExpectedReturn_IsRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new Borrowing(
                CreateStudent(),
                CreateEquipment(),
                new DateOnly(2026, 11, 18),
                new DateOnly(2026, 11, 17)));
    }

    [Fact]
    public void SameDayExpectedReturn_IsRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new Borrowing(
                CreateStudent(),
                CreateEquipment(),
                new DateOnly(2026, 11, 18),
                new DateOnly(2026, 11, 18)));
    }

    [Fact]
    public void ReturningActiveBorrowing_RecordsActualDate()
    {
        var borrowing = CreateBorrowing();

        borrowing.MarkAsReturned(
            new DateOnly(2026, 11, 20));

        Assert.Equal(BorrowingStatus.Returned, borrowing.Status);
        Assert.Equal(
            new DateOnly(2026, 11, 20),
            borrowing.ActualReturnDate);
    }

    [Fact]
    public void EarlyReturn_IsAllowed()
    {
        var borrowing = CreateBorrowing();

        borrowing.MarkAsReturned(
            new DateOnly(2026, 11, 18));

        Assert.Equal(BorrowingStatus.Returned, borrowing.Status);
    }

    [Fact]
    public void ReturnBeforeCheckout_IsRejected()
    {
        var borrowing = CreateBorrowing();

        Assert.Throws<ArgumentException>(() =>
            borrowing.MarkAsReturned(
                new DateOnly(2026, 11, 17)));

        Assert.Equal(BorrowingStatus.Active, borrowing.Status);
        Assert.Null(borrowing.ActualReturnDate);
    }

    [Fact]
    public void RepeatedReturn_IsRejected()
    {
        var borrowing = CreateBorrowing();

        borrowing.MarkAsReturned(
            new DateOnly(2026, 11, 20));

        Assert.Throws<InvalidOperationException>(() =>
            borrowing.MarkAsReturned(
                new DateOnly(2026, 11, 21)));

        Assert.Equal(
            new DateOnly(2026, 11, 20),
            borrowing.ActualReturnDate);
    }
}

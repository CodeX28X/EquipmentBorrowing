using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;

namespace EquipmentBorrowing.Tests.Domain;

public class BorrowingTests
{
    private static Student CreateStudent()
    {
        return new Student(
            1,
            "Juan Dela Cruz",
            2,
            true);
    }

    private static Equipment CreateEquipment()
    {
        return new Equipment(
            1,
            "Laptop");
    }

    [Fact]
    public void Constructor_WithValidInformation_CreatesActiveBorrowing()
    {
        var student = CreateStudent();
        var equipment = CreateEquipment();

        var borrowedAt = new DateTime(2026, 8, 27, 10, 0, 0);
        var expectedReturnDate = new DateTime(2026, 9, 3, 10, 0, 0);

        var borrowing = new Borrowing(
            student,
            equipment,
            borrowedAt,
            expectedReturnDate);

        Assert.Same(student, borrowing.Student);
        Assert.Same(equipment, borrowing.Equipment);
        Assert.Equal(borrowedAt, borrowing.BorrowedAt);
        Assert.Equal(expectedReturnDate, borrowing.ExpectedReturnDate);
        Assert.Equal(BorrowingStatus.Active, borrowing.Status);
    }

    [Fact]
    public void Constructor_WhenExpectedReturnDateIsEarlierThanBorrowedAt_ThrowsException()
    {
        var student = CreateStudent();
        var equipment = CreateEquipment();

        var borrowedAt = new DateTime(2026, 8, 27);
        var expectedReturnDate = new DateTime(2026, 8, 26);

        Assert.Throws<ArgumentException>(() =>
            new Borrowing(
                student,
                equipment,
                borrowedAt,
                expectedReturnDate));
    }

    [Fact]
    public void MarkAsReturned_WhenBorrowingIsActive_ChangesStatusToReturned()
    {
        var borrowing = new Borrowing(
            CreateStudent(),
            CreateEquipment(),
            new DateTime(2026, 8, 27),
            new DateTime(2026, 9, 3));

        borrowing.MarkAsReturned();

        Assert.Equal(BorrowingStatus.Returned, borrowing.Status);
    }

    [Fact]
    public void MarkAsReturned_WhenBorrowingIsAlreadyReturned_ThrowsException()
    {
        var borrowing = new Borrowing(
            CreateStudent(),
            CreateEquipment(),
            new DateTime(2026, 8, 27),
            new DateTime(2026, 9, 3));

        borrowing.MarkAsReturned();

        Assert.Throws<InvalidOperationException>(() =>
            borrowing.MarkAsReturned());
    }
}
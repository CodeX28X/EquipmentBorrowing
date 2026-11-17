
using EquipmentBorrowing.Domain.Enums;

namespace EquipmentBorrowing.Domain.Entities;

public class Borrowing
{
    public int BorrowingId { get; private set; }

    public int StudentId { get; private set; }

    public int EquipmentId { get; private set; }

    public Student Student { get; private set; } = null!;

    public Equipment Equipment { get; private set; } = null!;

    public DateOnly BorrowedAt { get; private set; }

    public DateOnly ExpectedReturnDate { get; private set; }

    public DateOnly? ActualReturnDate { get; private set; }

    public BorrowingStatus Status { get; private set; }

    // Required by EF Core.
    private Borrowing()
    {
    }

    public Borrowing(
        Student student,
        Equipment equipment,
        DateOnly borrowedAt,
        DateOnly expectedReturnDate)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(equipment);

        if (expectedReturnDate <= borrowedAt)
        {
            throw new ArgumentException(
                "Expected return date must be later than checkout date.",
                nameof(expectedReturnDate));
        }

        Student = student;
        StudentId = student.StudentId;

        Equipment = equipment;
        EquipmentId = equipment.EquipmentId;

        BorrowedAt = borrowedAt;
        ExpectedReturnDate = expectedReturnDate;

        ActualReturnDate = null;
        Status = BorrowingStatus.Active;
    }

    public void MarkAsReturned(DateOnly actualReturnDate)
    {
        if (Status != BorrowingStatus.Active)
        {
            throw new InvalidOperationException(
                "Only an active borrowing can be returned.");
        }

        if (actualReturnDate < BorrowedAt)
        {
            throw new ArgumentException(
                "Actual return date cannot precede checkout date.",
                nameof(actualReturnDate));
        }

        ActualReturnDate = actualReturnDate;
        Status = BorrowingStatus.Returned;
    }
}

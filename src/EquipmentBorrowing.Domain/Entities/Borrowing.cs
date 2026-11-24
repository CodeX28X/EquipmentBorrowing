
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

    // Reconstruct an HTTP read snapshot without changing persisted state.
    public static Borrowing FromReadSnapshot(
        int borrowingId, Student student, Equipment equipment,
        DateOnly borrowedDate, DateOnly expectedReturnDate,
        DateOnly? returnedDate, string status)
    {
        if (borrowingId <= 0)
            throw new ArgumentOutOfRangeException(nameof(borrowingId));

        var borrowing = new Borrowing(student, equipment, borrowedDate, expectedReturnDate)
        {
            BorrowingId = borrowingId
        };

        switch (status)
        {
            case "Active" when returnedDate is null:
                equipment.MarkAsBorrowed();
                break;
            case "Returned" when returnedDate.HasValue:
                borrowing.MarkAsReturned(returnedDate.Value);
                break;
            default:
                throw new ArgumentException("Invalid borrowing snapshot.", nameof(status));
        }
        return borrowing;
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

using EquipmentBorrowing.Domain.Enums;

namespace EquipmentBorrowing.Domain.Entities;

public class Borrowing
{
    public Student Student { get; }
    public Equipment Equipment { get; }
    public DateTime BorrowedAt { get; }
    public DateTime ExpectedReturnDate { get; }
    public BorrowingStatus Status { get; private set; }

    public Borrowing(
        Student student,
        Equipment equipment,
        DateTime borrowedAt,
        DateTime expectedReturnDate)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(equipment);

        if (expectedReturnDate < borrowedAt)
            throw new ArgumentException(
                "Expected return date cannot be earlier than the borrowed date.",
                nameof(expectedReturnDate));

        Student = student;
        Equipment = equipment;
        BorrowedAt = borrowedAt;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }

    public void MarkAsReturned()
    {
        if (Status == BorrowingStatus.Returned)
            throw new InvalidOperationException("Borrowing has already been returned.");

        Status = BorrowingStatus.Returned;
    }
}
namespace EquipmentBorrowing.Application.Services;

using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

public sealed class BorrowEquipmentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly int _maximumActiveBorrowings;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository,
        int maximumActiveBorrowings)
    {
        if (maximumActiveBorrowings <= 0)
        {
            throw new ArgumentException(
                "Maximum active borrowings must be greater than zero.",
                nameof(maximumActiveBorrowings));
        }

        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
        _maximumActiveBorrowings = maximumActiveBorrowings;
    }

    public async Task<BorrowResult> BorrowAsync(
        int studentId,
        int equipmentId,
        DateTime borrowedAt,
        DateTime expectedReturnDate,
        CancellationToken cancellationToken = default)
    {
        // 1. Find the student.
        Student? student =
            await _studentRepository.GetByIdAsync(
                studentId,
                cancellationToken);

        if (student is null)
        {
            return BorrowResult.Failure(
                "Student does not exist.");
        }

        // 2. Check whether the student is allowed to borrow.
        if (!student.IsAllowedToBorrow)
        {
            return BorrowResult.Failure(
                "Student is not allowed to borrow equipment.");
        }

        // 3. Check the student's current active borrowing count.
        int activeBorrowingCount =
            await _borrowingRepository.CountActiveByStudentAsync(
                studentId,
                cancellationToken);

        if (activeBorrowingCount >= _maximumActiveBorrowings)
        {
            return BorrowResult.Failure(
                "Student has reached the maximum number of active borrowings.");
        }

        // 4. Find the equipment.
        Equipment? equipment =
            await _equipmentRepository.GetByIdAsync(
                equipmentId,
                cancellationToken);

        if (equipment is null)
        {
            return BorrowResult.Failure(
                "Equipment does not exist.");
        }

        // 5. Check equipment availability.
        if (!equipment.IsAvailable)
        {
            return BorrowResult.Failure(
                "Equipment is currently unavailable.");
        }

        // 6. Create the borrowing domain object.
        Borrowing borrowing = new Borrowing(
            student,
            equipment,
            borrowedAt,
            expectedReturnDate);

        // 7. Change the equipment's domain state.
        equipment.MarkAsBorrowed();

        // 8. Persist the changes.
        await _borrowingRepository.AddAsync(
            borrowing,
            cancellationToken);

        await _equipmentRepository.UpdateAsync(
            equipment,
            cancellationToken);

        // 9. Return the successful result.
        return BorrowResult.Success(borrowing);
    }
}
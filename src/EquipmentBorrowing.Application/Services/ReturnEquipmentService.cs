using EquipmentBorrowing.Application;

namespace EquipmentBorrowing.Application.Services;

public sealed class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository)
    {
        _borrowingRepository = borrowingRepository;
    }

    public async Task<ReturnResult> ReturnAsync(
        int studentId,
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        var borrowing = await _borrowingRepository
            .GetActiveByStudentAndEquipmentAsync(
                studentId,
                equipmentId,
                cancellationToken);

        if (borrowing is null)
        {
            return ReturnResult.Failure(
                "No active borrowing was found for the specified student and equipment.");
        }


        var manilaTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

        var localNow = TimeZoneInfo.ConvertTime(
            DateTimeOffset.UtcNow,
            manilaTimeZone);

        var actualReturnDate = DateOnly.FromDateTime(
            localNow.DateTime);

        borrowing.MarkAsReturned(actualReturnDate);

        borrowing.Equipment.MarkAsAvailable();

        await _borrowingRepository.SaveChangesAsync(cancellationToken);

        return ReturnResult.Success(borrowing);
    }
}

namespace EquipmentBorrowing.Application;

public enum BorrowingOperationStatus
{
    Success,
    Rejected,
    Failed,
    OutcomeUnknown
}

public sealed record BorrowingOperationResult(
    BorrowingOperationStatus Status,
    string Message,
    IReadOnlyList<int> BorrowingIds)
{

    public OperationErrorKind ErrorKind { get; init; }

    public bool IsSuccess =>
        Status == BorrowingOperationStatus.Success;

    public static BorrowingOperationResult Success(
        string message,
        IReadOnlyList<int> borrowingIds) =>
        new(BorrowingOperationStatus.Success, message, borrowingIds);

    public static BorrowingOperationResult Rejected(
        string message,
        OperationErrorKind errorKind =
            OperationErrorKind.Validation) =>
        new(BorrowingOperationStatus.Rejected, message, [])
        {
            ErrorKind = errorKind
        };

    public static BorrowingOperationResult Failed(
        string message) =>
        new(BorrowingOperationStatus.Failed, message, [])
        {
            ErrorKind = OperationErrorKind.ServerError
        };

    public static BorrowingOperationResult Unknown() =>
        new(
            BorrowingOperationStatus.OutcomeUnknown,
            "The database may have committed the operation. " +
            "Refresh the borrowing records before attempting it again.",
            [])
        {
            ErrorKind = OperationErrorKind.OutcomeUnknown
        };
}

public interface IAtomicBorrowingService
{
    Task<BorrowingOperationResult> CheckoutAsync(
        int studentId,
        IReadOnlyCollection<int> equipmentIds,
        DateOnly checkoutDate,
        DateOnly expectedReturnDate,
        CancellationToken cancellationToken = default);

    Task<BorrowingOperationResult> ReturnAsync(
        int studentId,
        int equipmentId,
        DateOnly actualReturnDate,
        CancellationToken cancellationToken = default);

    Task<BorrowingOperationResult> ReturnByIdAsync(
        int borrowingId,
        DateOnly actualReturnDate,
        CancellationToken cancellationToken = default);
}

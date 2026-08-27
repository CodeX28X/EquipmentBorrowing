using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Services;

public sealed record BorrowResult(
    bool IsSuccess,
    string Message,
    Borrowing? Borrowing = null)
{
    public static BorrowResult Success(
        Borrowing borrowing)
    {
        return new BorrowResult(
            true,
            "Equipment borrowed successfully.",
            borrowing);
    }

    public static BorrowResult Failure(
        string message)
    {
        return new BorrowResult(
            false,
            message);
    }
}
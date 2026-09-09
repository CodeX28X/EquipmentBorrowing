using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Services;

public sealed record ReturnResult(
    bool IsSuccess,
    string Message,
    Borrowing? Borrowing = null)
{
    public static ReturnResult Success(
        Borrowing borrowing)
    {
        return new ReturnResult(
            true,
            "Equipment returned successfully.",
            borrowing);
    }

    public static ReturnResult Failure(
        string message)
    {
        return new ReturnResult(
            false,
            message);
    }
}
using System.ComponentModel.DataAnnotations;
namespace EquipmentBorrowing.Api.Contracts;
public sealed record BorrowingResponse(int Id, int StudentId, int EquipmentId, DateOnly BorrowedDate,
    DateOnly ExpectedReturnDate, DateOnly? ReturnedDate, string Status, string StudentNumber,
    string StudentName, string EquipmentCode, string EquipmentName);
public sealed class BorrowingRequest
{
    public required int StudentId { get; init; }
    public required int EquipmentId { get; init; }
    public required DateOnly ExpectedReturnDate { get; init; }
}
public sealed class CheckoutRequest
{
    public required int StudentId { get; init; }
    [Required, MinLength(2)] public required int[] EquipmentIds { get; init; }
    public required DateOnly ExpectedReturnDate { get; init; }
}
public sealed class ReturnRequest { }
public sealed record CheckoutResponse(BorrowingResponse[] Borrowings);

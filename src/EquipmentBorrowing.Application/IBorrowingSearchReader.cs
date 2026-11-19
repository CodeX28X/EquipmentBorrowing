using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application;

public enum BorrowingSearchStatus
{
    All,
    Active,
    Returned
}

public enum BorrowingSortField
{
    CheckoutDate,
    ExpectedReturnDate
}

public enum BorrowingSortDirection
{
    Ascending,
    Descending
}

public sealed record BorrowingSearchCriteria(
    string? SearchText,
    BorrowingSearchStatus Status,
    BorrowingSortField SortField,
    BorrowingSortDirection Direction);

/// <summary>Read-only, database-backed search for UC-09.</summary>
public interface IBorrowingSearchReader
{
    Task<IReadOnlyList<Borrowing>> SearchAsync(
        BorrowingSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}

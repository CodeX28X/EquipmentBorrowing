using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>Applies all UC-09 criteria in PostgreSQL before materializing results.</summary>
public sealed class EfBorrowingSearchReader : IBorrowingSearchReader
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _factory;

    public EfBorrowingSearchReader(
        IDbContextFactory<EquipmentBorrowingDbContext> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task<IReadOnlyList<Borrowing>> SearchAsync(
        BorrowingSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        IQueryable<Borrowing> query = db.Borrowings.AsNoTracking();

        string term = (criteria.SearchText ?? string.Empty).Trim().ToLowerInvariant();
        if (term.Length > 0)
        {
            // Contains uses literal partial matching (not SQL wildcard matching).
            // All three related fields participate in the same OR predicate.
            query = query.Where(b =>
                b.Student.StudentName.ToLower().Contains(term) ||
                b.Student.StudentNumber.ToLower().Contains(term) ||
                b.Equipment.EquipmentName.ToLower().Contains(term));
        }

        query = criteria.Status switch
        {
            BorrowingSearchStatus.Active =>
                query.Where(b => b.Status == BorrowingStatus.Active),
            BorrowingSearchStatus.Returned =>
                query.Where(b => b.Status == BorrowingStatus.Returned),
            BorrowingSearchStatus.All => query,
            _ => throw new ArgumentOutOfRangeException(nameof(criteria))
        };

        IOrderedQueryable<Borrowing> ordered =
            (criteria.SortField, criteria.Direction) switch
            {
                (BorrowingSortField.CheckoutDate, BorrowingSortDirection.Ascending) =>
                    query.OrderBy(b => b.BorrowedAt),
                (BorrowingSortField.CheckoutDate, BorrowingSortDirection.Descending) =>
                    query.OrderByDescending(b => b.BorrowedAt),
                (BorrowingSortField.ExpectedReturnDate, BorrowingSortDirection.Ascending) =>
                    query.OrderBy(b => b.ExpectedReturnDate),
                (BorrowingSortField.ExpectedReturnDate, BorrowingSortDirection.Descending) =>
                    query.OrderByDescending(b => b.ExpectedReturnDate),
                _ => throw new ArgumentOutOfRangeException(nameof(criteria))
            };

        // Stable, deterministic ordering for equal dates.
        return await ordered.ThenBy(b => b.BorrowingId)
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .ToListAsync(cancellationToken);
    }
}

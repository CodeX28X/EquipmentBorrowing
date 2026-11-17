
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfActiveBorrowingsReader
    : IActiveBorrowingsReader
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext>
        _contextFactory;

    public EfActiveBorrowingsReader(
        IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
    {
        _contextFactory = contextFactory
            ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public async Task<IReadOnlyList<Borrowing>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await _contextFactory.CreateDbContextAsync(
                cancellationToken);

        return await db.Borrowings
            .AsNoTracking()
            .Where(b => b.Status == BorrowingStatus.Active)
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .OrderByDescending(b => b.BorrowedAt)
            .ThenBy(b => b.BorrowingId)
            .ToListAsync(cancellationToken);
    }
}

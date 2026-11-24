using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EquipmentBorrowing.Infrastructure.Repositories;
public sealed class EfBorrowingDetailsReader(IDbContextFactory<EquipmentBorrowingDbContext> factory) : IBorrowingDetailsReader
{
    public async Task<Borrowing?> GetByIdAsync(int borrowingId, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        return await db.Borrowings.AsNoTracking().Include(b => b.Student).Include(b => b.Equipment)
            .SingleOrDefaultAsync(b => b.BorrowingId == borrowingId, cancellationToken);
    }
}

using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>Uses a fresh context per query; never modifies historical records.</summary>
public sealed class EfBorrowingHistoryReader : IBorrowingHistoryReader
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _factory;

    public EfBorrowingHistoryReader(
        IDbContextFactory<EquipmentBorrowingDbContext> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task<IReadOnlyList<Borrowing>> GetForStudentAsync(
        int studentId, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Borrowings.AsNoTracking()
            .Where(b => b.StudentId == studentId)
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .OrderByDescending(b => b.BorrowedAt)
            .ThenByDescending(b => b.BorrowingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetForEquipmentAsync(
        int equipmentId, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Borrowings.AsNoTracking()
            .Where(b => b.EquipmentId == equipmentId)
            .Include(b => b.Student)
            .Include(b => b.Equipment)
            .OrderByDescending(b => b.BorrowedAt)
            .ThenByDescending(b => b.BorrowingId)
            .ToListAsync(cancellationToken);
    }
}

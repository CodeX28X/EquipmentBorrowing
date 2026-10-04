using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfBorrowingRepository : IBorrowingRepository
{
    private readonly EquipmentBorrowingDbContext _dbContext;

    public EfBorrowingRepository(
        EquipmentBorrowingDbContext dbContext)
    {
        _dbContext = dbContext;
    }



    public async Task SaveChangesAsync(
    CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }



    public async Task<int> CountActiveByStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Borrowings
            .CountAsync(
                borrowing =>
                    borrowing.Student.StudentId == studentId &&
                    borrowing.Status == BorrowingStatus.Active,
                cancellationToken);
    }

    public async Task AddAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Borrowings.Add(borrowing);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Borrowings
            .Where(borrowing => borrowing.Status == BorrowingStatus.Active)
            .Include(borrowing => borrowing.Student)
            .Include(borrowing => borrowing.Equipment)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Borrowing?> GetActiveByStudentAndEquipmentAsync(
        int studentId,
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Borrowings
            .Where(
                borrowing =>
                    borrowing.Student.StudentId == studentId &&
                    borrowing.Equipment.EquipmentId == equipmentId &&
                    borrowing.Status == BorrowingStatus.Active)
            .Include(borrowing => borrowing.Student)
            .Include(borrowing => borrowing.Equipment)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
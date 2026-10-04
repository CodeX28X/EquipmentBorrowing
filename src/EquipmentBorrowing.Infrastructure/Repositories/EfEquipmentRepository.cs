using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfEquipmentRepository : IEquipmentRepository
{
    private readonly EquipmentBorrowingDbContext _dbContext;

    public EfEquipmentRepository(EquipmentBorrowingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Equipment?> GetByIdAsync(
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Equipment
            .FirstOrDefaultAsync(
                equipment => equipment.EquipmentId == equipmentId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Equipment
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
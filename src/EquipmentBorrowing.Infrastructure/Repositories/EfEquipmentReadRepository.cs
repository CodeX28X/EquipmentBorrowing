
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>
/// Provides independent, read-only equipment queries.
/// </summary>
public sealed class EfEquipmentReadRepository : IEquipmentRepository
{
	private readonly IDbContextFactory<EquipmentBorrowingDbContext>
		_contextFactory;

	public EfEquipmentReadRepository(
		IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
	{
		_contextFactory = contextFactory
			?? throw new ArgumentNullException(nameof(contextFactory));
	}

	public async Task<Equipment?> GetByIdAsync(
		int equipmentId,
		CancellationToken cancellationToken = default)
	{
		await using var dbContext =
			await _contextFactory.CreateDbContextAsync(
				cancellationToken);

		return await dbContext.Equipment
			.AsNoTracking()
			.FirstOrDefaultAsync(
				equipment => equipment.EquipmentId == equipmentId,
				cancellationToken);
	}

	public async Task<IReadOnlyList<Equipment>> GetAllAsync(
		CancellationToken cancellationToken = default)
	{
		await using var dbContext =
			await _contextFactory.CreateDbContextAsync(
				cancellationToken);

		return await dbContext.Equipment
			.AsNoTracking()
			.OrderBy(equipment => equipment.EquipmentName)
			.ThenBy(equipment => equipment.EquipmentId)
			.ToListAsync(cancellationToken);
	}
}

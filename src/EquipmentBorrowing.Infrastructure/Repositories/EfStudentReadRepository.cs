
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>
/// Provides independent, read-only student queries.
/// Each operation creates and disposes its own DbContext.
/// </summary>
public sealed class EfStudentReadRepository : IStudentRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext>
        _contextFactory;

    public EfStudentReadRepository(
        IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
    {
        _contextFactory = contextFactory
            ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public async Task<Student?> GetByIdAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _contextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(
                student => student.StudentId == studentId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _contextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.Students
            .AsNoTracking()
            .OrderBy(student => student.StudentName)
            .ThenBy(student => student.StudentId)
            .ToListAsync(cancellationToken);
    }
}

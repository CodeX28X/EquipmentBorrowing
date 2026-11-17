
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

/// <summary>
/// Creates independent database contexts for runtime operations.
/// The caller is responsible for disposing each created context.
/// </summary>
public sealed class EquipmentBorrowingContextFactory
    : IDbContextFactory<EquipmentBorrowingDbContext>
{
    private readonly DbContextOptions<EquipmentBorrowingDbContext>
        _options;

    public EquipmentBorrowingContextFactory(
        DbContextOptions<EquipmentBorrowingDbContext> options)
    {
        _options = options
            ?? throw new ArgumentNullException(nameof(options));
    }

    public EquipmentBorrowingDbContext CreateDbContext()
    {
        return new EquipmentBorrowingDbContext(_options);
    }

    public Task<EquipmentBorrowingDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(CreateDbContext());
    }
}

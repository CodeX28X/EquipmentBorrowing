
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public sealed class EquipmentBorrowingDbContextFactory
    : IDesignTimeDbContextFactory<EquipmentBorrowingDbContext>
{
    public EquipmentBorrowingDbContext CreateDbContext(
        string[] args)
    {
        const string variableName =
            "EQUIPMENTBORROWING_CONNECTION_STRING";

        string? connectionString =
            Environment.GetEnvironmentVariable(variableName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Missing {variableName}. " +
                "Set the PostgreSQL connection string " +
                "in your local environment before running " +
                "EF Core migration commands.");
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<EquipmentBorrowingDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new EquipmentBorrowingDbContext(
            optionsBuilder.Options);
    }
}

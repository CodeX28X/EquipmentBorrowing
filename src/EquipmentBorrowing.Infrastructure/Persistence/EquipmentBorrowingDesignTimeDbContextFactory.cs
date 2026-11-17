
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public sealed class EquipmentBorrowingDesignTimeDbContextFactory
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
                $"The environment variable {variableName} " +
                "must contain a PostgreSQL connection string.");
        }

        var options =
            new DbContextOptionsBuilder<EquipmentBorrowingDbContext>()
                .UseNpgsql(connectionString)
                .Options;

        return new EquipmentBorrowingDbContext(options);
    }
}

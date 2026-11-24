using EquipmentBorrowing.Application;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using EquipmentBorrowing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEquipmentBorrowingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString =
            configuration["EQUIPMENTBORROWING_CONNECTION_STRING"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing EQUIPMENTBORROWING_CONNECTION_STRING. " +
                "Configure the PostgreSQL connection string.");
        }

        services.AddDbContextFactory<EquipmentBorrowingDbContext>(
            options => options.UseNpgsql(connectionString));

        // Read-only repositories.
        services.AddScoped<IStudentRepository,
            EfStudentReadRepository>();

        services.AddScoped<IEquipmentRepository,
            EfEquipmentReadRepository>();

        services.AddScoped<IActiveBorrowingsReader,
            EfActiveBorrowingsReader>();

        services.AddScoped<IBorrowingHistoryReader,
            EfBorrowingHistoryReader>();

        services.AddScoped<IBorrowingSearchReader,
            EfBorrowingSearchReader>();

        services.AddScoped<IBorrowingDetailsReader,
            EfBorrowingDetailsReader>();

        // Transactional application services.
        services.AddScoped<IAtomicBorrowingService,
            PostgreSqlAtomicBorrowingService>();

        services.AddScoped<IStudentManagementService,
            PostgreSqlStudentManagementService>();

        services.AddScoped<IEquipmentManagementService,
            PostgreSqlEquipmentManagementService>();

        return services;
    }
}
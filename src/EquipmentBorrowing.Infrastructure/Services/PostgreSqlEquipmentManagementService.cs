
using System;
using System.Linq;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EquipmentBorrowing.Infrastructure.Services;

public sealed class PostgreSqlEquipmentManagementService
    : IEquipmentManagementService
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext>
        _contextFactory;

    public PostgreSqlEquipmentManagementService(
        IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
    {
        _contextFactory = contextFactory
            ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string? ValidateFields(
        string? equipmentIdentifier,
        string? equipmentName)
    {
        if (string.IsNullOrWhiteSpace(equipmentIdentifier))
            return "Equipment identifier is required.";

        if (string.IsNullOrWhiteSpace(equipmentName))
            return "Equipment name is required.";

        return null;
    }

    private static bool IsDuplicateIdentifier(Exception exception)
    {
        if (exception is PostgresException postgres)
            return postgres.SqlState ==
                PostgresErrorCodes.UniqueViolation;

        return exception.InnerException is not null &&
            IsDuplicateIdentifier(exception.InnerException);
    }

    private async Task<EquipmentOperationResult> ExecuteAsync(
        Func<EquipmentBorrowingDbContext,
            CancellationToken,
            Task<(int EquipmentId, string Message)>> operation,
        CancellationToken cancellationToken)
    {
        bool commitAttempted = false;

        try
        {
            await using var db =
                await _contextFactory.CreateDbContextAsync(
                    cancellationToken);

            await using var transaction =
                await db.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted,
                    cancellationToken);

            var (equipmentId, message) =
                await operation(db, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            commitAttempted = true;
            await transaction.CommitAsync(cancellationToken);

            return EquipmentOperationResult.Success(
                equipmentId,
                message);
        }
        catch (Exception) when (commitAttempted)
        {
            return EquipmentOperationResult.Unknown();
        }
        catch (Exception exception)
            when (IsDuplicateIdentifier(exception))
        {
            return EquipmentOperationResult.Rejected(
                "This equipment identifier is already registered.");
        }
        catch (ArgumentException exception)
        {
            return EquipmentOperationResult.Rejected(
                exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            if (exception.Message == "Equipment does not exist.")
                return EquipmentOperationResult.Rejected(
                    exception.Message);

            return EquipmentOperationResult.Failed(
                "Equipment operation could not be completed.");
        }
        catch (Exception)
        {
            return EquipmentOperationResult.Failed(
                "The database operation failed before commit. " +
                "Refresh equipment records before retrying.");
        }
    }

    public Task<EquipmentOperationResult> RegisterAsync(
        string equipmentIdentifier,
        string equipmentName,
        string? description,
        CancellationToken cancellationToken = default)
    {
        string? validationError =
            ValidateFields(equipmentIdentifier, equipmentName);

        if (validationError is not null)
            return Task.FromResult(
                EquipmentOperationResult.Rejected(validationError));

        string identifier = equipmentIdentifier.Trim();
        string name = equipmentName.Trim();
        string? normalizedDescription =
            NormalizeOptional(description);

        return ExecuteAsync(
            async (db, ct) =>
            {
                int equipmentId = await db.Database
                    .SqlQueryRaw<int>(
                        """
                        SELECT nextval(
                            'public."EquipmentIdSequence"'::regclass
                        )::integer AS "Value"
                        """)
                    .SingleAsync(ct);

                var equipment = new Equipment(
                    equipmentId,
                    identifier,
                    name,
                    normalizedDescription);

                db.Equipment.Add(equipment);

                return (
                    equipmentId,
                    "Equipment registered successfully."
                );
            },
            cancellationToken);
    }

    public Task<EquipmentOperationResult> UpdateAsync(
        int equipmentId,
        string equipmentIdentifier,
        string equipmentName,
        string? description,
        CancellationToken cancellationToken = default)
    {
        if (equipmentId <= 0)
            return Task.FromResult(
                EquipmentOperationResult.Rejected(
                    "Select valid equipment."));

        string? validationError =
            ValidateFields(equipmentIdentifier, equipmentName);

        if (validationError is not null)
            return Task.FromResult(
                EquipmentOperationResult.Rejected(validationError));

        string identifier = equipmentIdentifier.Trim();
        string name = equipmentName.Trim();
        string? normalizedDescription =
            NormalizeOptional(description);

        return ExecuteAsync(
            async (db, ct) =>
            {
                var equipmentItems = await db.Equipment
                    .FromSqlInterpolated(
                        $"""
                        SELECT * FROM "Equipment"
                        WHERE "EquipmentId" = {equipmentId}
                        FOR UPDATE
                        """)
                    .AsTracking()
                    .ToListAsync(ct);

                Equipment? equipment =
                    equipmentItems.SingleOrDefault();

                if (equipment is null)
                    throw new InvalidOperationException(
                        "Equipment does not exist.");

                equipment.UpdateInformation(
                    identifier,
                    name,
                    normalizedDescription);

                return (
                    equipment.EquipmentId,
                    "Equipment information updated successfully."
                );
            },
            cancellationToken);
    }
}

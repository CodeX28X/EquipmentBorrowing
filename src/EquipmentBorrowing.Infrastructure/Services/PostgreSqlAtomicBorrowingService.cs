
using System.Data;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Services;

public sealed class PostgreSqlAtomicBorrowingService
    : IAtomicBorrowingService
{
    private const int MaximumActiveBorrowings = 3;

    private readonly IDbContextFactory<EquipmentBorrowingDbContext>
        _contextFactory;

    public PostgreSqlAtomicBorrowingService(
        IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
    {
        _contextFactory = contextFactory
            ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public async Task<BorrowingOperationResult> CheckoutAsync(
        int studentId,
        IReadOnlyCollection<int> equipmentIds,
        DateOnly checkoutDate,
        DateOnly expectedReturnDate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(equipmentIds);

        if (studentId <= 0)
            return BorrowingOperationResult.Rejected(
                "Select a valid student.");

        if (equipmentIds.Count == 0)
            return BorrowingOperationResult.Rejected(
                "Select at least one equipment item.");

        if (equipmentIds.Any(id => id <= 0))
            return BorrowingOperationResult.Rejected(
                "Equipment IDs must be positive.");

        if (equipmentIds.Count != equipmentIds.Distinct().Count())
            return BorrowingOperationResult.Rejected(
                "Duplicate equipment selections are not allowed.");

        if (equipmentIds.Count > MaximumActiveBorrowings)
            return BorrowingOperationResult.Rejected(
                "A student may borrow at most three active items.");

        if (expectedReturnDate <= checkoutDate)
            return BorrowingOperationResult.Rejected(
                "Expected return date must be later than checkout date.");

        var orderedIds = equipmentIds.OrderBy(id => id).ToArray();

        await using var db =
            await _contextFactory.CreateDbContextAsync(
                cancellationToken);

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

        bool commitAttempted = false;

        try
        {
            // Lock the student first. Checkout operations for the
            // same student will be serialized by this row lock.
            var students = await db.Students
                .FromSqlInterpolated(
                    $"""
                    SELECT * FROM "Students"
                    WHERE "StudentId" = {studentId}
                    FOR UPDATE
                    """)
                .AsTracking()
                .ToListAsync(cancellationToken);

            var student = students.SingleOrDefault();

            if (student is null)
                return BorrowingOperationResult.Rejected(
                    "Student does not exist.");

            if (!student.IsAllowedToBorrow)
                return BorrowingOperationResult.Rejected(
                    "Student is not eligible to borrow.");

            int activeCount = await db.Borrowings
                .CountAsync(
                    b => b.StudentId == studentId &&
                         b.Status == BorrowingStatus.Active,
                    cancellationToken);

            if (activeCount + orderedIds.Length >
                MaximumActiveBorrowings)
            {
                return BorrowingOperationResult.Rejected(
                    "Checkout would exceed the three-item limit.");
            }

            var selectedEquipment = new List<Equipment>();

            // Always acquire equipment locks in ascending ID order
            // to reduce the possibility of deadlocks.
            foreach (int equipmentId in orderedIds)
            {
                var items = await db.Equipment
                    .FromSqlInterpolated(
                        $"""
                        SELECT * FROM "Equipment"
                        WHERE "EquipmentId" = {equipmentId}
                        FOR UPDATE
                        """)
                    .AsTracking()
                    .ToListAsync(cancellationToken);

                var equipment = items.SingleOrDefault();

                if (equipment is null)
                    return BorrowingOperationResult.Rejected(
                        $"Equipment {equipmentId} does not exist.");

                if (!equipment.IsAvailable)
                    return BorrowingOperationResult.Rejected(
                        $"Equipment {equipmentId} is unavailable.");

                bool hasActiveLoan = await db.Borrowings
                    .AnyAsync(
                        b => b.EquipmentId == equipmentId &&
                             b.Status == BorrowingStatus.Active,
                        cancellationToken);

                if (hasActiveLoan)
                    return BorrowingOperationResult.Rejected(
                        $"Equipment {equipmentId} has an active loan.");

                selectedEquipment.Add(equipment);
            }

            var newBorrowings = new List<Borrowing>();

            foreach (var equipment in selectedEquipment)
            {
                var borrowing = new Borrowing(
                    student,
                    equipment,
                    checkoutDate,
                    expectedReturnDate);

                equipment.MarkAsBorrowed();

                db.Borrowings.Add(borrowing);
                newBorrowings.Add(borrowing);
            }

            // One SaveChanges call writes all selected loans
            // and corresponding availability changes.
            await db.SaveChangesAsync(cancellationToken);

            // Never automatically retry after this point:
            // the commit acknowledgement might be lost.
            commitAttempted = true;
            await transaction.CommitAsync(cancellationToken);

            return BorrowingOperationResult.Success(
                "Equipment checkout completed.",
                newBorrowings
                    .Select(b => b.BorrowingId)
                    .ToArray());
        }
        catch (Exception) when (commitAttempted)
        {
            return BorrowingOperationResult.Unknown();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (DbUpdateException)
        {
            return BorrowingOperationResult.Failed(
                "Checkout could not be saved. Refresh the records " +
                "and verify equipment availability.");
        }
    }

    public async Task<BorrowingOperationResult> ReturnAsync(
        int studentId,
        int equipmentId,
        DateOnly actualReturnDate,
        CancellationToken cancellationToken = default)
    {
        if (studentId <= 0 || equipmentId <= 0)
            return BorrowingOperationResult.Rejected(
                "Select a valid student and equipment item.");

        await using var db =
            await _contextFactory.CreateDbContextAsync(
                cancellationToken);

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

        bool commitAttempted = false;

        try
        {
            // Maintain the same lock order as checkout:
            // student, then equipment.
            var students = await db.Students
                .FromSqlInterpolated(
                    $"""
                    SELECT * FROM "Students"
                    WHERE "StudentId" = {studentId}
                    FOR UPDATE
                    """)
                .AsTracking()
                .ToListAsync(cancellationToken);

            if (students.Count == 0)
                return BorrowingOperationResult.Rejected(
                    "Student does not exist.");

            // Eligibility must NOT block returning equipment.
            var items = await db.Equipment
                .FromSqlInterpolated(
                    $"""
                    SELECT * FROM "Equipment"
                    WHERE "EquipmentId" = {equipmentId}
                    FOR UPDATE
                    """)
                .AsTracking()
                .ToListAsync(cancellationToken);

            var equipment = items.SingleOrDefault();

            if (equipment is null)
                return BorrowingOperationResult.Rejected(
                    "Equipment does not exist.");

            var borrowing = await db.Borrowings
                .Include(b => b.Student)
                .Include(b => b.Equipment)
                .FirstOrDefaultAsync(
                    b => b.StudentId == studentId &&
                         b.EquipmentId == equipmentId &&
                         b.Status == BorrowingStatus.Active,
                    cancellationToken);

            if (borrowing is null)
                return BorrowingOperationResult.Rejected(
                    "No active borrowing exists for this student and item.");

            if (actualReturnDate < borrowing.BorrowedAt)
                return BorrowingOperationResult.Rejected(
                    "Return date cannot precede checkout date.");

            borrowing.MarkAsReturned(actualReturnDate);
            equipment.MarkAsAvailable();

            await db.SaveChangesAsync(cancellationToken);

            commitAttempted = true;
            await transaction.CommitAsync(cancellationToken);

            return BorrowingOperationResult.Success(
                "Equipment returned successfully.",
                [borrowing.BorrowingId]);
        }
        catch (Exception) when (commitAttempted)
        {
            return BorrowingOperationResult.Unknown();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (DbUpdateException)
        {
            return BorrowingOperationResult.Failed(
                "Return could not be saved. Refresh the records " +
                "before trying again.");
        }
    }
}

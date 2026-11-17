
using System.Data;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EquipmentBorrowing.Infrastructure.Services;

public sealed class PostgreSqlStudentManagementService
    : IStudentManagementService
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext>
        _contextFactory;

    public PostgreSqlStudentManagementService(
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
        string? studentNumber,
        string? studentName)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
            return "Student number is required.";

        if (string.IsNullOrWhiteSpace(studentName))
            return "Student name is required.";

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

    private async Task<StudentOperationResult> ExecuteAsync(
        Func<EquipmentBorrowingDbContext,
            CancellationToken,
            Task<(int StudentId, string Message)>> operation,
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

            var (studentId, message) =
                await operation(db, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            commitAttempted = true;
            await transaction.CommitAsync(cancellationToken);

            return StudentOperationResult.Success(
                studentId,
                message);
        }
        catch (Exception) when (commitAttempted)
        {
            return StudentOperationResult.Unknown();
        }
        catch (Exception exception)
            when (IsDuplicateIdentifier(exception))
        {
            return StudentOperationResult.Rejected(
                "This student number is already registered.");
        }
        catch (ArgumentException exception)
        {
            return StudentOperationResult.Rejected(
                exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            // Used below for an absent student.
            // Other unexpected errors will be represented as failures.
            if (exception.Message == "Student does not exist.")
                return StudentOperationResult.Rejected(
                    exception.Message);

            return StudentOperationResult.Failed(
                "Student operation could not be completed.");
        }
        catch (Exception)
        {
            return StudentOperationResult.Failed(
                "The database operation failed before commit. " +
                "Refresh student records before retrying.");
        }
    }

    public Task<StudentOperationResult> RegisterAsync(
        string studentNumber,
        string studentName,
        string? contactInformation,
        int? studentYear,
        CancellationToken cancellationToken = default)
    {
        string? validationError =
            ValidateFields(studentNumber, studentName);

        if (validationError is not null)
            return Task.FromResult(
                StudentOperationResult.Rejected(validationError));

        string number = studentNumber.Trim();
        string name = studentName.Trim();
        string? contact = NormalizeOptional(contactInformation);

        return ExecuteAsync(
            async (db, ct) =>
            {
                // PostgreSQL allocates the next internal ID.
                int studentId = await db.Database
                    .SqlQueryRaw<int>(
                        """
                        SELECT nextval(
                            'public."StudentIdSequence"'::regclass
                        )::integer AS "Value"
                        """)
                    .SingleAsync(ct);

                var student = new Student(
                    studentId,
                    number,
                    name,
                    contact,
                    studentYear);

                // The domain constructor defaults eligibility to true.
                db.Students.Add(student);

                return (
                    studentId,
                    "Student registered successfully."
                );
            },
            cancellationToken);
    }

    public Task<StudentOperationResult> UpdateAsync(
        int studentId,
        string studentNumber,
        string studentName,
        string? contactInformation,
        int? studentYear,
        CancellationToken cancellationToken = default)
    {
        if (studentId <= 0)
            return Task.FromResult(
                StudentOperationResult.Rejected(
                    "Select a valid student."));

        string? validationError =
            ValidateFields(studentNumber, studentName);

        if (validationError is not null)
            return Task.FromResult(
                StudentOperationResult.Rejected(validationError));

        string number = studentNumber.Trim();
        string name = studentName.Trim();
        string? contact = NormalizeOptional(contactInformation);

        return ExecuteAsync(
            async (db, ct) =>
            {
                var students = await db.Students
                    .FromSqlInterpolated(
                        $"""
                        SELECT * FROM "Students"
                        WHERE "StudentId" = {studentId}
                        FOR UPDATE
                        """)
                    .AsTracking()
                    .ToListAsync(ct);

                Student? student = students.SingleOrDefault();

                if (student is null)
                    throw new InvalidOperationException(
                        "Student does not exist.");

                student.UpdateInformation(
                    number,
                    name,
                    contact,
                    studentYear);

                return (
                    student.StudentId,
                    "Student information updated successfully."
                );
            },
            cancellationToken);
    }

    public Task<StudentOperationResult> SetEligibilityAsync(
        int studentId,
        bool isAllowedToBorrow,
        CancellationToken cancellationToken = default)
    {
        if (studentId <= 0)
            return Task.FromResult(
                StudentOperationResult.Rejected(
                    "Select a valid student."));

        return ExecuteAsync(
            async (db, ct) =>
            {
                var students = await db.Students
                    .FromSqlInterpolated(
                        $"""
                        SELECT * FROM "Students"
                        WHERE "StudentId" = {studentId}
                        FOR UPDATE
                        """)
                    .AsTracking()
                    .ToListAsync(ct);

                Student? student = students.SingleOrDefault();

                if (student is null)
                    throw new InvalidOperationException(
                        "Student does not exist.");

                student.SetBorrowingEligibility(
                    isAllowedToBorrow);

                return (
                    student.StudentId,
                    isAllowedToBorrow
                        ? "Student borrowing eligibility enabled."
                        : "Student borrowing eligibility disabled."
                );
            },
            cancellationToken);
    }
}

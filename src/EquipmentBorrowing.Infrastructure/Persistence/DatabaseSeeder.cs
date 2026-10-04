using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;


public static class DatabaseSeeder
{
    public static async Task InitializeAsync(
        EquipmentBorrowingDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        // Apply existing migrations without deleting existing data.
        await dbContext.Database.MigrateAsync(cancellationToken);

        // ---------------------------------------------------------
        // STUDENTS
        // ---------------------------------------------------------

        var students = new[]




        {
            new Student(1, "Juan Dela Cruz", 3, true),
            new Student(2, "Maria Santos", 2, true),
            new Student(3, "Pedro Reyes", 1, false),
            new Student(4, "Ana Garcia", 4, true)


        };

        foreach (var student in students)
        {
            var exists = await dbContext.Students
                .AnyAsync(
                    existing => existing.StudentId == student.StudentId,
                    cancellationToken);

            if (!exists)
            {
                dbContext.Students.Add(student);
            }
        }

        // ---------------------------------------------------------
        // EQUIPMENT
        // ---------------------------------------------------------

        var equipment = new[]
        {
            new Equipment(1, "Laptop 01"),
            new Equipment(2, "Laptop 02"),
            new Equipment(3, "Projector 01"),
            new Equipment(4, "Camera 01")
        };

        foreach (var item in equipment)
        {
            var exists = await dbContext.Equipment
                .AnyAsync(
                    existing => existing.EquipmentId == item.EquipmentId,
                    cancellationToken);

            if (!exists)
            {
                dbContext.Equipment.Add(item);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // ---------------------------------------------------------
        // ACTIVE BORROWING SCENARIO
        // ---------------------------------------------------------
        // Equipment 1 is intentionally unavailable because it is
        // currently borrowed by Student 1.

        var existingBorrowing =
            await dbContext.Borrowings
                .AnyAsync(
                    borrowing =>
                        borrowing.Student.StudentId == 1 &&
                        borrowing.Equipment.EquipmentId == 1,
                    cancellationToken);


        if (!existingBorrowing)
        {
            var student = await dbContext.Students
                .SingleAsync(
                    student => student.StudentId == 1,
                    cancellationToken);

            var item = await dbContext.Equipment
                .SingleAsync(
                    equipment => equipment.EquipmentId == 1,
                    cancellationToken);

            if (item.IsAvailable)
            {
                item.MarkAsBorrowed();

                var borrowedAt = DateTime.Now;
                var expectedReturnDate = borrowedAt.AddDays(7);

                var borrowing = new Borrowing(
                    student,
                    item,
                    borrowedAt,
                    expectedReturnDate);

                dbContext.Borrowings.Add(borrowing);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
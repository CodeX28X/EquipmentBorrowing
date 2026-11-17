
using EquipmentBorrowing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    // Call explicitly only after applying the correct
    // PostgreSQL migrations.
    // Do not invoke automatically during normal startup.
    public static async Task InitializeAsync(
        EquipmentBorrowingDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        // Never overwrite a database that already has data.
        bool hasStudents = await dbContext.Students
            .AnyAsync(cancellationToken);

        bool hasEquipment = await dbContext.Equipment
            .AnyAsync(cancellationToken);

        bool hasBorrowings = await dbContext.Borrowings
            .AnyAsync(cancellationToken);

        if (hasStudents || hasEquipment || hasBorrowings)
        {
            return;
        }

        var students = new[]
        {
            new Student(
                1, "STU-0001", "Juan Dela Cruz",
                studentYear: 3),

            new Student(
                2, "STU-0002", "Maria Santos",
                studentYear: 2),

            new Student(
                3, "STU-0003", "Pedro Reyes",
                studentYear: 1),

            new Student(
                4, "STU-0004", "Ana Garcia",
                studentYear: 4)
        };

        // Sample student who cannot borrow.
        students[2].SetBorrowingEligibility(false);

        var equipment = new[]
        {
            new Equipment(
                1, "LAP-001", "Laptop 01"),

            new Equipment(
                2, "LAP-002", "Laptop 02"),

            new Equipment(
                3, "PROJ-001", "Projector 01"),

            new Equipment(
                4, "CAM-001", "Camera 01")
        };

        dbContext.Students.AddRange(students);
        dbContext.Equipment.AddRange(equipment);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

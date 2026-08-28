using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

// 1. Create sample data

Student student = new Student(
    studentId: 1,
    studentName: "Juan Dela Cruz",
    studentYear: 2,
    isAllowedToBorrow: true);

Equipment equipment = new Equipment(
    equipmentId: 1,
    equipmentName: "Laptop");

// 2. Create repository implementations

IStudentRepository studentRepository =
    new InMemoryStudentRepository(
        new[] { student });

IEquipmentRepository equipmentRepository =
    new InMemoryEquipmentRepository(
        new[] { equipment });

IBorrowingRepository borrowingRepository =
    new InMemoryBorrowingRepository();


// 3. Create the application service

BorrowEquipmentService borrowingService =
    new BorrowEquipmentService(
        studentRepository,
        equipmentRepository,
        borrowingRepository,
        maximumActiveBorrowings: 3);

// 4. Execute a successful borrowing request

Console.WriteLine("=== Equipment Borrowing System ===");
Console.WriteLine();

Console.WriteLine("Attempting to borrow equipment...");

BorrowResult result =
    await borrowingService.BorrowAsync(
        studentId: 1,
        equipmentId: 1,
        borrowedAt: DateTime.Now,
        expectedReturnDate: DateTime.Now.AddDays(7));

Console.WriteLine($"Success: {result.IsSuccess}");
Console.WriteLine($"Message: {result.Message}");

// 5. Display resulting domain state

Console.WriteLine();
Console.WriteLine("=== Resulting State ===");

Console.WriteLine(
    $"Student: {student.StudentName}");

Console.WriteLine(
    $"Equipment: {equipment.EquipmentName}");

Console.WriteLine(
    $"Equipment Available: {equipment.IsAvailable}");

if (result.Borrowing is not null)
{
    Console.WriteLine(
        $"Borrowing Status: {result.Borrowing.Status}");

    Console.WriteLine(
        $"Borrowed At: {result.Borrowing.BorrowedAt}");

    Console.WriteLine(
        $"Expected Return Date: {result.Borrowing.ExpectedReturnDate}");
}

// 6. Execute an unsuccessful borrowing request

Console.WriteLine();
Console.WriteLine("=== Second Borrowing Attempt ===");
Console.WriteLine();

BorrowResult secondResult =
    await borrowingService.BorrowAsync(
        studentId: 1,
        equipmentId: 1,
        borrowedAt: DateTime.Now,
        expectedReturnDate: DateTime.Now.AddDays(7));

Console.WriteLine($"Success: {secondResult.IsSuccess}");
Console.WriteLine($"Message: {secondResult.Message}");
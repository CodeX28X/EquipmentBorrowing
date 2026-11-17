
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

// Create sample data using the Lab 4 constructors.

var student = new Student(
    studentId: 1,
    studentNumber: "STU-0001",
    studentName: "Juan Dela Cruz",
    studentYear: 2);

var equipment = new Equipment(
    equipmentId: 1,
    equipmentIdentifier: "LAP-001",
    equipmentName: "Laptop");

// Use in-memory repositories for this console demonstration.

IStudentRepository studentRepository =
    new InMemoryStudentRepository(new[] { student });

IEquipmentRepository equipmentRepository =
    new InMemoryEquipmentRepository(new[] { equipment });

IBorrowingRepository borrowingRepository =
    new InMemoryBorrowingRepository();

var borrowingService = new BorrowEquipmentService(
    studentRepository,
    equipmentRepository,
    borrowingRepository,
    maximumActiveBorrowings: 3);

// Lab 4 uses local calendar dates.

var manilaTimeZone =
    TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

var manilaNow = TimeZoneInfo.ConvertTime(
    DateTimeOffset.UtcNow,
    manilaTimeZone);

var checkoutDate = DateOnly.FromDateTime(
    manilaNow.DateTime);

var expectedReturnDate = checkoutDate.AddDays(7);

Console.WriteLine("=== Equipment Borrowing System ===");
Console.WriteLine();

Console.WriteLine("Attempting to borrow equipment...");

BorrowResult result = await borrowingService.BorrowAsync(
    studentId: student.StudentId,
    equipmentId: equipment.EquipmentId,
    borrowedAt: checkoutDate.ToDateTime(TimeOnly.MinValue),
    expectedReturnDate:
        expectedReturnDate.ToDateTime(TimeOnly.MinValue));

Console.WriteLine($"Success: {result.IsSuccess}");
Console.WriteLine($"Message: {result.Message}");

Console.WriteLine();
Console.WriteLine("=== Resulting State ===");

Console.WriteLine($"Student: {student.StudentName}");
Console.WriteLine($"Student Number: {student.StudentNumber}");
Console.WriteLine($"Equipment: {equipment.EquipmentName}");
Console.WriteLine($"Equipment Available: {equipment.IsAvailable}");

if (result.Borrowing is not null)
{
    Console.WriteLine(
        $"Borrowing Status: {result.Borrowing.Status}");

    Console.WriteLine(
        $"Checkout Date: {result.Borrowing.BorrowedAt}");

    Console.WriteLine(
        $"Expected Return: {result.Borrowing.ExpectedReturnDate}");
}

// Verify duplicate borrowing is rejected.

Console.WriteLine();
Console.WriteLine("=== Second Borrowing Attempt ===");

BorrowResult secondResult = await borrowingService.BorrowAsync(
    studentId: student.StudentId,
    equipmentId: equipment.EquipmentId,
    borrowedAt: checkoutDate.ToDateTime(TimeOnly.MinValue),
    expectedReturnDate:
        expectedReturnDate.ToDateTime(TimeOnly.MinValue));

Console.WriteLine($"Success: {secondResult.IsSuccess}");
Console.WriteLine($"Message: {secondResult.Message}");

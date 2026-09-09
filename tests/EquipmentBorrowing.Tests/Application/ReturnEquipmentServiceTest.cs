using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests;

public class ReturnEquipmentServiceTests
{
    [Fact]
    public async Task ReturnAsync_WithActiveBorrowing_ReturnsSuccess()
    {
        // Arrange
        var student = new Student(
            1,
            "Juan Dela Cruz",
            2,
            true);

        var equipment = new Equipment(
            101,
            "Laptop");

        var borrowing = new Borrowing(
            student,
            equipment,
            DateTime.Now,
            DateTime.Now.AddDays(7));

        equipment.MarkAsBorrowed();

        var repository = new InMemoryBorrowingRepository(
            new[] { borrowing });

        var service = new ReturnEquipmentService(repository);

        // Act
        var result = await service.ReturnAsync(
            student.StudentId,
            equipment.EquipmentId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            "Equipment returned successfully.",
            result.Message);
        Assert.NotNull(result.Borrowing);
    }

    [Fact]
    public async Task ReturnAsync_WithActiveBorrowing_MarksBorrowingAsReturned()
    {
        // Arrange
        var student = new Student(
            1,
            "Juan Dela Cruz",
            2,
            true);

        var equipment = new Equipment(
            101,
            "Laptop");

        var borrowing = new Borrowing(
            student,
            equipment,
            DateTime.Now,
            DateTime.Now.AddDays(7));

        equipment.MarkAsBorrowed();

        var repository = new InMemoryBorrowingRepository(
            new[] { borrowing });

        var service = new ReturnEquipmentService(repository);

        // Act
        await service.ReturnAsync(
            student.StudentId,
            equipment.EquipmentId);

        // Assert
        Assert.Equal(
            BorrowingStatus.Returned,
            borrowing.Status);
    }

    [Fact]
    public async Task ReturnAsync_WithActiveBorrowing_MakesEquipmentAvailable()
    {
        // Arrange
        var student = new Student(
            1,
            "Juan Dela Cruz",
            2,
            true);

        var equipment = new Equipment(
            101,
            "Laptop");

        var borrowing = new Borrowing(
            student,
            equipment,
            DateTime.Now,
            DateTime.Now.AddDays(7));

        equipment.MarkAsBorrowed();

        var repository = new InMemoryBorrowingRepository(
            new[] { borrowing });

        var service = new ReturnEquipmentService(repository);

        // Act
        await service.ReturnAsync(
            student.StudentId,
            equipment.EquipmentId);

        // Assert
        Assert.True(equipment.IsAvailable);
    }

    [Fact]
    public async Task ReturnAsync_WhenNoActiveBorrowingExists_ReturnsFailure()
    {
        // Arrange
        var repository = new InMemoryBorrowingRepository();

        var service = new ReturnEquipmentService(repository);

        // Act
        var result = await service.ReturnAsync(
            999,
            999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Borrowing);
        Assert.Equal(
            "No active borrowing was found for the specified student and equipment.",
            result.Message);
    }

    [Fact]
    public async Task ReturnAsync_WhenBorrowingWasAlreadyReturned_ReturnsFailure()
    {
        // Arrange
        var student = new Student(
            1,
            "Juan Dela Cruz",
            2,
            true);

        var equipment = new Equipment(
            101,
            "Laptop");

        var borrowing = new Borrowing(
            student,
            equipment,
            DateTime.Now,
            DateTime.Now.AddDays(7));

        equipment.MarkAsBorrowed();
        borrowing.MarkAsReturned();
        equipment.MarkAsAvailable();

        var repository = new InMemoryBorrowingRepository(
            new[] { borrowing });

        var service = new ReturnEquipmentService(repository);

        // Act
        var result = await service.ReturnAsync(
            student.StudentId,
            equipment.EquipmentId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Borrowing);
    }
}
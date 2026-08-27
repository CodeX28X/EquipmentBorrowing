using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests.Application;

public class BorrowEquipmentServiceTest
{
	[Fact]
	public async Task BorrowAsync_WithValidStudentAndAvailableEquipment_ReturnsSuccess()
	{
		// Arrange
		Student student = new Student(
			1,
			"John Doe",
			2,
			true);

		Equipment equipment = new Equipment(
			1,
			"Laptop");

		InMemoryStudentRepository studentRepository =
			new InMemoryStudentRepository(
				new[] { student });

		InMemoryEquipmentRepository equipmentRepository =
			new InMemoryEquipmentRepository(
				new[] { equipment });

		InMemoryBorrowingRepository borrowingRepository =
			new InMemoryBorrowingRepository();

		BorrowEquipmentService service =
			new BorrowEquipmentService(
				studentRepository,
				equipmentRepository,
				borrowingRepository,
				maximumActiveBorrowings: 3);

		DateTime borrowedAt =
			new DateTime(2026, 8, 28, 9, 0, 0);

		DateTime expectedReturnDate =
			borrowedAt.AddDays(3);

		// Act
		BorrowResult result = await service.BorrowAsync(
			student.StudentId,
			equipment.EquipmentId,
			borrowedAt,
			expectedReturnDate);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Borrowing);

		Assert.Equal(
			student.StudentId,
			result.Borrowing.Student.StudentId);

		Assert.Equal(
			equipment.EquipmentId,
			result.Borrowing.Equipment.EquipmentId);

		Assert.False(equipment.IsAvailable);
	}

	[Fact]
	public async Task BorrowAsync_WhenStudentDoesNotExist_ReturnsFailure()
	{
		// Arrange
		Equipment equipment = new Equipment(
			1,
			"Laptop");

		InMemoryStudentRepository studentRepository =
			new InMemoryStudentRepository();

		InMemoryEquipmentRepository equipmentRepository =
			new InMemoryEquipmentRepository(
				new[] { equipment });

		InMemoryBorrowingRepository borrowingRepository =
			new InMemoryBorrowingRepository();

		BorrowEquipmentService service =
			new BorrowEquipmentService(
				studentRepository,
				equipmentRepository,
				borrowingRepository,
				maximumActiveBorrowings: 3);

		DateTime borrowedAt =
			new DateTime(2026, 8, 28, 9, 0, 0);

		DateTime expectedReturnDate =
			borrowedAt.AddDays(3);

		// Act
		BorrowResult result = await service.BorrowAsync(
			999,
			equipment.EquipmentId,
			borrowedAt,
			expectedReturnDate);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Null(result.Borrowing);
	}

	[Fact]
	public async Task BorrowAsync_WhenStudentIsNotAllowedToBorrow_ReturnsFailure()
	{
		// Arrange
		Student student = new Student(
			1,
			"John Doe",
			2,
			false);

		Equipment equipment = new Equipment(
			1,
			"Laptop");

		InMemoryStudentRepository studentRepository =
			new InMemoryStudentRepository(
				new[] { student });

		InMemoryEquipmentRepository equipmentRepository =
			new InMemoryEquipmentRepository(
				new[] { equipment });

		InMemoryBorrowingRepository borrowingRepository =
			new InMemoryBorrowingRepository();

		BorrowEquipmentService service =
			new BorrowEquipmentService(
				studentRepository,
				equipmentRepository,
				borrowingRepository,
				maximumActiveBorrowings: 3);

		DateTime borrowedAt =
			new DateTime(2026, 8, 28, 9, 0, 0);

		DateTime expectedReturnDate =
			borrowedAt.AddDays(3);

		// Act
		BorrowResult result = await service.BorrowAsync(
			student.StudentId,
			equipment.EquipmentId,
			borrowedAt,
			expectedReturnDate);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Null(result.Borrowing);
		Assert.True(equipment.IsAvailable);
	}

	[Fact]
	public async Task BorrowAsync_WhenStudentReachedBorrowingLimit_ReturnsFailure()
	{
		// Arrange
		Student student = new Student(
			1,
			"John Doe",
			2,
			true);

		Equipment equipment1 = new Equipment(1, "Laptop");
		Equipment equipment2 = new Equipment(2, "Projector");
		Equipment equipment3 = new Equipment(3, "Camera");
		Equipment equipment4 = new Equipment(4, "Microphone");

		Borrowing borrowing1 = new Borrowing(
			student,
			equipment1,
			new DateTime(2026, 8, 20, 9, 0, 0),
			new DateTime(2026, 8, 23, 9, 0, 0));

		Borrowing borrowing2 = new Borrowing(
			student,
			equipment2,
			new DateTime(2026, 8, 21, 9, 0, 0),
			new DateTime(2026, 8, 24, 9, 0, 0));

		Borrowing borrowing3 = new Borrowing(
			student,
			equipment3,
			new DateTime(2026, 8, 22, 9, 0, 0),
			new DateTime(2026, 8, 25, 9, 0, 0));

		InMemoryStudentRepository studentRepository =
			new InMemoryStudentRepository(
				new[] { student });

		InMemoryEquipmentRepository equipmentRepository =
			new InMemoryEquipmentRepository(
				new[] { equipment4 });

		InMemoryBorrowingRepository borrowingRepository =
			new InMemoryBorrowingRepository(
				new[] { borrowing1, borrowing2, borrowing3 });

		BorrowEquipmentService service =
			new BorrowEquipmentService(
				studentRepository,
				equipmentRepository,
				borrowingRepository,
				maximumActiveBorrowings: 3);

		DateTime borrowedAt =
			new DateTime(2026, 8, 28, 9, 0, 0);

		DateTime expectedReturnDate =
			borrowedAt.AddDays(3);

		// Act
		BorrowResult result = await service.BorrowAsync(
			student.StudentId,
			equipment4.EquipmentId,
			borrowedAt,
			expectedReturnDate);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Null(result.Borrowing);
		Assert.True(equipment4.IsAvailable);
	}

	[Fact]
	public async Task BorrowAsync_WhenEquipmentDoesNotExist_ReturnsFailure()
	{
		// Arrange
		Student student = new Student(
			1,
			"John Doe",
			2,
			true);

		InMemoryStudentRepository studentRepository =
			new InMemoryStudentRepository(
				new[] { student });

		InMemoryEquipmentRepository equipmentRepository =
			new InMemoryEquipmentRepository();

		InMemoryBorrowingRepository borrowingRepository =
			new InMemoryBorrowingRepository();

		BorrowEquipmentService service =
			new BorrowEquipmentService(
				studentRepository,
				equipmentRepository,
				borrowingRepository,
				maximumActiveBorrowings: 3);

		DateTime borrowedAt =
			new DateTime(2026, 8, 28, 9, 0, 0);

		DateTime expectedReturnDate =
			borrowedAt.AddDays(3);

		// Act
		BorrowResult result = await service.BorrowAsync(
			student.StudentId,
			999,
			borrowedAt,
			expectedReturnDate);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Null(result.Borrowing);
	}

	[Fact]
	public async Task BorrowAsync_WhenEquipmentIsUnavailable_ReturnsFailure()
	{
		// Arrange
		Student student = new Student(
			1,
			"John Doe",
			2,
			true);

		Equipment equipment = new Equipment(
			1,
			"Laptop");

		equipment.MarkAsBorrowed();

		InMemoryStudentRepository studentRepository =
			new InMemoryStudentRepository(
				new[] { student });

		InMemoryEquipmentRepository equipmentRepository =
			new InMemoryEquipmentRepository(
				new[] { equipment });

		InMemoryBorrowingRepository borrowingRepository =
			new InMemoryBorrowingRepository();

		BorrowEquipmentService service =
			new BorrowEquipmentService(
				studentRepository,
				equipmentRepository,
				borrowingRepository,
				maximumActiveBorrowings: 3);

		DateTime borrowedAt =
			new DateTime(2026, 8, 28, 9, 0, 0);

		DateTime expectedReturnDate =
			borrowedAt.AddDays(3);

		// Act
		BorrowResult result = await service.BorrowAsync(
			student.StudentId,
			equipment.EquipmentId,
			borrowedAt,
			expectedReturnDate);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Null(result.Borrowing);
		Assert.False(equipment.IsAvailable);
	}

	[Fact]
	public async Task BorrowAsync_WhenExpectedReturnDateIsBeforeBorrowedAt_ThrowsArgumentException()
	{
		// Arrange
		Student student = new Student(
			1,
			"John Doe",
			2,
			true);

		Equipment equipment = new Equipment(
			1,
			"Laptop");

		InMemoryStudentRepository studentRepository =
			new InMemoryStudentRepository(
				new[] { student });

		InMemoryEquipmentRepository equipmentRepository =
			new InMemoryEquipmentRepository(
				new[] { equipment });

		InMemoryBorrowingRepository borrowingRepository =
			new InMemoryBorrowingRepository();

		BorrowEquipmentService service =
			new BorrowEquipmentService(
				studentRepository,
				equipmentRepository,
				borrowingRepository,
				maximumActiveBorrowings: 3);

		DateTime borrowedAt =
			new DateTime(2026, 8, 28, 9, 0, 0);

		DateTime expectedReturnDate =
			new DateTime(2026, 8, 27, 9, 0, 0);

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentException>(
			() => service.BorrowAsync(
				student.StudentId,
				equipment.EquipmentId,
				borrowedAt,
				expectedReturnDate));
	}
}


using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests.Infrastructure.Repositories;

public class InMemoryBorrowingRepositoryTests
{
	[Fact]
	public async Task CountActiveByStudentAsync_WhenNoBorrowingsExist_ReturnsZero()
	{
		// Arrange
		InMemoryBorrowingRepository repository =
			new InMemoryBorrowingRepository();

		// Act
		int count =
			await repository.CountActiveByStudentAsync(1);

		// Assert
		Assert.Equal(0, count);
	}

	[Fact]
	public async Task CountActiveByStudentAsync_WhenStudentHasActiveBorrowing_ReturnsCorrectCount()
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

		Borrowing borrowing = new Borrowing(
			student,
			equipment,
			new DateTime(2026, 8, 28, 9, 0, 0),
			new DateTime(2026, 8, 30, 9, 0, 0));

		InMemoryBorrowingRepository repository =
			new InMemoryBorrowingRepository(
				new[] { borrowing });

		// Act
		int count =
			await repository.CountActiveByStudentAsync(
				student.StudentId);

		// Assert
		Assert.Equal(1, count);
	}

	[Fact]
	public async Task CountActiveByStudentAsync_WhenBorrowingIsReturned_DoesNotCountIt()
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

		Borrowing borrowing = new Borrowing(
			student,
			equipment,
			new DateTime(2026, 8, 28, 9, 0, 0),
			new DateTime(2026, 8, 30, 9, 0, 0));

		borrowing.MarkAsReturned();

		InMemoryBorrowingRepository repository =
			new InMemoryBorrowingRepository(
				new[] { borrowing });

		// Act
		int count =
			await repository.CountActiveByStudentAsync(
				student.StudentId);

		// Assert
		Assert.Equal(0, count);
	}

	[Fact]
	public async Task AddAsync_WhenBorrowingIsAdded_IncreasesActiveCount()
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

		Borrowing borrowing = new Borrowing(
			student,
			equipment,
			new DateTime(2026, 8, 28, 9, 0, 0),
			new DateTime(2026, 8, 30, 9, 0, 0));

		InMemoryBorrowingRepository repository =
			new InMemoryBorrowingRepository();

		// Act
		await repository.AddAsync(borrowing);

		int count =
			await repository.CountActiveByStudentAsync(
				student.StudentId);

		// Assert
		Assert.Equal(1, count);
	}
}
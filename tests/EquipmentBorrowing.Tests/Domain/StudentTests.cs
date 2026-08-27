using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Tests.Domain;

public class StudentTests
{
	[Fact]
	public void Constructor_WithValidInformation_CreatesStudent()
	{
		var student = new Student(
			1,
			"Juan Dela Cruz",
			2,
			true);

		Assert.Equal(1, student.StudentId);
		Assert.Equal("Juan Dela Cruz", student.StudentName);
		Assert.Equal(2, student.StudentYear);
		Assert.True(student.IsAllowedToBorrow);
	}

	[Fact]
	public void Constructor_WithInvalidStudentId_ThrowsException()
	{
		Assert.Throws<ArgumentException>(() =>
			new Student(
				0,
				"Juan Dela Cruz",
				2,
				true));
	}

	[Fact]
	public void Constructor_WithEmptyStudentName_ThrowsException()
	{
		Assert.Throws<ArgumentException>(() =>
			new Student(
				1,
				"",
				2,
				true));
	}

	[Fact]
	public void Constructor_WithInvalidStudentYear_ThrowsException()
	{
		Assert.Throws<ArgumentException>(() =>
			new Student(
				1,
				"Juan Dela Cruz",
				0,
				true));
	}
}
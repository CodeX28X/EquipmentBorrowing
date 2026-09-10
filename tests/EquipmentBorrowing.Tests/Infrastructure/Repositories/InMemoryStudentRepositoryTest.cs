using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests.Infrastructure.Repositories;

public class InMemoryStudentRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_WhenStudentExists_ReturnsStudent()
    {
        // Arrange
        Student student = new Student(
            1,
            "John Doe",
            2,
            true);

        InMemoryStudentRepository repository =
            new InMemoryStudentRepository(
                new[] { student });

        // Act
        Student? result =
            await repository.GetByIdAsync(student.StudentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(student.StudentId, result.StudentId);
        Assert.Equal(student.StudentName, result.StudentName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenStudentDoesNotExist_ReturnsNull()
    {
        // Arrange
        InMemoryStudentRepository repository =
            new InMemoryStudentRepository();

        // Act
        Student? result =
            await repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WhenStudentsExist_ReturnsAllStudents()
    {
        Student student1 = new Student(
            1,
            "Stephen",
            2,
            true);

        Student student2 = new Student(
            2,
            "Scudge",
            2,
            true);

        InMemoryStudentRepository repository =
            new InMemoryStudentRepository(
                new[] { student1, student2 });

        IReadOnlyList<Student> result =
            await repository.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenStudentsExist_ReturnsMatchingStudents()
    {
        Student student1 = new Student(
            1,
            "Stephen",
            2,
            true);

        Student student2 = new Student(
            2,
            "Scudge",
            2,
            true);

        InMemoryStudentRepository repository =
            new InMemoryStudentRepository(
                new[] { student1, student2 });

        IReadOnlyList<Student> result =
            await repository.GetAllAsync();

        Assert.Equal(
            student1.StudentId,
            result[0].StudentId);

        Assert.Equal(
            student1.StudentName,
            result[0].StudentName);

        Assert.Equal(
            student2.StudentId,
            result[1].StudentId);

        Assert.Equal(
            student2.StudentName,
            result[1].StudentName);
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryIsEmpty_ReturnsEmptyCollection()
    {
        InMemoryStudentRepository repository =
            new InMemoryStudentRepository();

        IReadOnlyList<Student> result =
            await repository.GetAllAsync();

        Assert.Empty(result);
    }
}
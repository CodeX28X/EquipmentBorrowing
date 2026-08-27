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
}
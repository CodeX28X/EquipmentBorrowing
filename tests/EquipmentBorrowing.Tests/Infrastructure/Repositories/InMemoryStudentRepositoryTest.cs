
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests.Infrastructure.Repositories;

public class InMemoryStudentRepositoryTests
{
    private static Student Create(int id) =>
        new(id, $"STU-{id:000}", $"Student {id}");

    [Fact]
    public async Task ExistingStudent_IsRetrievedById()
    {
        var student = Create(1);
        var repo = new InMemoryStudentRepository(
            new[] { student });

        var result = await repo.GetByIdAsync(1);

        Assert.Same(student, result);
        Assert.Equal("STU-001", result!.StudentNumber);
    }

    [Fact]
    public async Task MissingStudent_ReturnsNull()
    {
        var repo = new InMemoryStudentRepository();

        var result = await repo.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAll_ReturnsEveryStudent()
    {
        var first = Create(1);
        var second = Create(2);
        var repo = new InMemoryStudentRepository(
            new[] { first, second });

        var results = await repo.GetAllAsync();

        Assert.Equal(2, results.Count);
        Assert.Contains(first, results);
        Assert.Contains(second, results);
    }

    [Fact]
    public async Task GetAll_PreservesStudentDetails()
    {
        var student = Create(1);
        var repo = new InMemoryStudentRepository(
            new[] { student });

        var result = Assert.Single(
            await repo.GetAllAsync());

        Assert.Equal(student.StudentId, result.StudentId);
        Assert.Equal(student.StudentNumber,
            result.StudentNumber);
        Assert.Equal(student.StudentName,
            result.StudentName);
    }

    [Fact]
    public async Task EmptyRepository_ReturnsEmptyCollection()
    {
        var repo = new InMemoryStudentRepository();

        Assert.Empty(await repo.GetAllAsync());
    }
}


using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests.Infrastructure.Repositories;

public class InMemoryEquipmentRepositoryTests
{
    private static Equipment Create(int id) =>
        new(id, $"EQ-{id:000}", $"Equipment {id}");

    [Fact]
    public async Task ExistingEquipment_IsRetrievedById()
    {
        var item = Create(1);
        var repo = new InMemoryEquipmentRepository(
            new[] { item });

        var result = await repo.GetByIdAsync(1);

        Assert.Same(item, result);
        Assert.Equal("EQ-001",
            result!.EquipmentIdentifier);
    }

    [Fact]
    public async Task MissingEquipment_ReturnsNull()
    {
        var repo = new InMemoryEquipmentRepository();

        Assert.Null(await repo.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetAll_ReturnsEveryItem()
    {
        var first = Create(1);
        var second = Create(2);
        var repo = new InMemoryEquipmentRepository(
            new[] { first, second });

        var results = await repo.GetAllAsync();

        Assert.Equal(2, results.Count);
        Assert.Contains(first, results);
        Assert.Contains(second, results);
    }

    [Fact]
    public async Task GetAll_PreservesEquipmentDetails()
    {
        var item = Create(1);
        var repo = new InMemoryEquipmentRepository(
            new[] { item });

        var result = Assert.Single(
            await repo.GetAllAsync());

        Assert.Equal(item.EquipmentId,
            result.EquipmentId);
        Assert.Equal(item.EquipmentIdentifier,
            result.EquipmentIdentifier);
        Assert.Equal(item.EquipmentName,
            result.EquipmentName);
        Assert.True(result.IsAvailable);
    }

    [Fact]
    public async Task EmptyRepository_ReturnsEmptyCollection()
    {
        var repo = new InMemoryEquipmentRepository();

        Assert.Empty(await repo.GetAllAsync());
    }
}

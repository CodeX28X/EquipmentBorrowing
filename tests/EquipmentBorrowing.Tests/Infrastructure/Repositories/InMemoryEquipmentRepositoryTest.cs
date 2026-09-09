using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests.Infrastructure.Repositories;

public class InMemoryEquipmentRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_WhenEquipmentExists_ReturnsEquipment()
    {
        // Arrange
        Equipment equipment = new Equipment(
            1,
            "Laptop");

        InMemoryEquipmentRepository repository =
            new InMemoryEquipmentRepository(
                new[] { equipment });

        // Act
        Equipment? result =
            await repository.GetByIdAsync(equipment.EquipmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(
            equipment.EquipmentId,
            result.EquipmentId);

        Assert.Equal(
            equipment.EquipmentName,
            result.EquipmentName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEquipmentDoesNotExist_ReturnsNull()
    {
        // Arrange
        InMemoryEquipmentRepository repository =
            new InMemoryEquipmentRepository();

        // Act
        Equipment? result =
            await repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WhenEquipmentExists_ReturnsAllEquipment()
    {
        // Arrange
        Equipment equipment1 = new Equipment(
            1,
            "Laptop");

        Equipment equipment2 = new Equipment(
            2,
            "Projector");

        InMemoryEquipmentRepository repository =
            new InMemoryEquipmentRepository(
                new[] { equipment1, equipment2 });

        // Act
        IReadOnlyList<Equipment> result =
            await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenEquipmentExists_ReturnsMatchingEquipment()
    {
        // Arrange
        Equipment equipment1 = new Equipment(
            1,
            "Laptop");

        Equipment equipment2 = new Equipment(
            2,
            "Projector");

        InMemoryEquipmentRepository repository =
            new InMemoryEquipmentRepository(
                new[] { equipment1, equipment2 });

        // Act
        IReadOnlyList<Equipment> result =
            await repository.GetAllAsync();

        // Assert
        Assert.Equal(
            equipment1.EquipmentId,
            result[0].EquipmentId);

        Assert.Equal(
            equipment1.EquipmentName,
            result[0].EquipmentName);

        Assert.Equal(
            equipment2.EquipmentId,
            result[1].EquipmentId);

        Assert.Equal(
            equipment2.EquipmentName,
            result[1].EquipmentName);
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryIsEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        InMemoryEquipmentRepository repository =
            new InMemoryEquipmentRepository();

        // Act
        IReadOnlyList<Equipment> result =
            await repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

}
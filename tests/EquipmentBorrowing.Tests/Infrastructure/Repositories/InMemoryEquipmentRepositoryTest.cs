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
}
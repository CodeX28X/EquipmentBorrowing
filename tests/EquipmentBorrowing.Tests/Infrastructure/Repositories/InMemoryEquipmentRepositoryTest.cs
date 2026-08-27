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
    public async Task UpdateAsync_WhenEquipmentExists_ReplacesEquipment()
    {
        // Arrange
        Equipment originalEquipment = new Equipment(
            1,
            "Laptop");

        Equipment updatedEquipment = new Equipment(
            1,
            "Updated Laptop");

        InMemoryEquipmentRepository repository =
            new InMemoryEquipmentRepository(
                new[] { originalEquipment });

        // Act
        await repository.UpdateAsync(updatedEquipment);

        Equipment? result =
            await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(
            "Updated Laptop",
            result.EquipmentName);
    }

    [Fact]
    public async Task UpdateAsync_WhenEquipmentDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        Equipment equipment = new Equipment(
            999,
            "Laptop");

        InMemoryEquipmentRepository repository =
            new InMemoryEquipmentRepository();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.UpdateAsync(equipment));
    }
}
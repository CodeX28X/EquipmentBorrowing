using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly List<Equipment> _equipment;

    public InMemoryEquipmentRepository(
        IEnumerable<Equipment>? equipment = null)
    {
        _equipment = equipment?.ToList() ?? new List<Equipment>();
    }

    public Task<Equipment?> GetByIdAsync(
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        Equipment? equipment = _equipment
            .FirstOrDefault(
                equipment => equipment.EquipmentId == equipmentId);

        return Task.FromResult(equipment);
    }

    public Task UpdateAsync(
        Equipment equipment,
        CancellationToken cancellationToken = default)
    {
        int existingIndex = _equipment.FindIndex(
            existing => existing.EquipmentId == equipment.EquipmentId);

        if (existingIndex == -1)
        {
            throw new InvalidOperationException(
                $"Equipment with ID {equipment.EquipmentId} does not exist.");
        }

        _equipment[existingIndex] = equipment;

        return Task.CompletedTask;
    }
}
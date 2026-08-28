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
}
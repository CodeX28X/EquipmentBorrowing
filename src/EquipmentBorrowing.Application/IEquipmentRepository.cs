using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(
        int equipmentId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Equipment equipment,
        CancellationToken cancellationToken = default);
}
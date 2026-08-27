namespace EquipmentBorrowing.Application.Interfaces;

using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Domain.Entities;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(
        int equipmentId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Equipment equipment,
        CancellationToken cancellationToken = default);
}
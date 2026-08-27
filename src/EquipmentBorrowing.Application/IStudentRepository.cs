namespace EquipmentBorrowing.Application.Interfaces;

using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Domain.Entities;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(
        int studentId,
        CancellationToken cancellationToken = default);
}
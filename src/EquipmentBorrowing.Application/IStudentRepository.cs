using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
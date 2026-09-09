using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application;

public interface IBorrowingRepository
{
    Task<int> CountActiveByStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<Borrowing?> GetActiveByStudentAndEquipmentAsync(
        int studentId,
        int equipmentId,
        CancellationToken cancellationToken = default);
}
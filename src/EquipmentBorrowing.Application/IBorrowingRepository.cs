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
}
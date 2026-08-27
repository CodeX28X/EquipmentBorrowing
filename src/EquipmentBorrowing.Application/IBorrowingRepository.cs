namespace EquipmentBorrowing.Application.Interfaces;

using EquipmentBorrowing.Domain.Entities;

public interface IBorrowingRepository
{
    Task<int> CountActiveByStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default);
}
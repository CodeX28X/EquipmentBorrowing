using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly List<Borrowing> _borrowings;

    public InMemoryBorrowingRepository(
        IEnumerable<Borrowing>? borrowings = null)
    {
        _borrowings = borrowings?.ToList() ?? new List<Borrowing>();
    }

    public Task<int> CountActiveByStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        int count = _borrowings.Count(
            borrowing =>
                borrowing.Student.StudentId == studentId &&
                borrowing.Status == BorrowingStatus.Active);

        return Task.FromResult(count);
    }

    public Task AddAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default)
    {
        _borrowings.Add(borrowing);

        return Task.CompletedTask;
    }
}
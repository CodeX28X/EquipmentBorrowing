using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application;

/// <summary>Read-only, complete borrowing histories for selected records.</summary>
public interface IBorrowingHistoryReader
{
    Task<IReadOnlyList<Borrowing>> GetForStudentAsync(
        int studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> GetForEquipmentAsync(
        int equipmentId, CancellationToken cancellationToken = default);
}

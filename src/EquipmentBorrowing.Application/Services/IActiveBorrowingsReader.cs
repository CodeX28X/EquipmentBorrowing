
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application;

public interface IActiveBorrowingsReader
{
    Task<IReadOnlyList<Borrowing>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}

using EquipmentBorrowing.Domain.Entities;
namespace EquipmentBorrowing.Application;
public interface IBorrowingDetailsReader
{
    Task<Borrowing?> GetByIdAsync(int borrowingId, CancellationToken cancellationToken = default);
}

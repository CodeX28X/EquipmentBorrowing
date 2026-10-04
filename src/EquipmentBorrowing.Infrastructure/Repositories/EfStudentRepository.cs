using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;



public sealed class EfStudentRepository : IStudentRepository
{
    private readonly EquipmentBorrowingDbContext _dbContext;



    public EfStudentRepository(EquipmentBorrowingDbContext dbContext)
    {
        _dbContext = dbContext;
    }



    public async Task<Student?> GetByIdAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Students
            .FirstOrDefaultAsync(
                student => student.StudentId == studentId,
                cancellationToken);
    }



    public async Task<IReadOnlyList<Student>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Students
            .ToListAsync(cancellationToken);
    }
}
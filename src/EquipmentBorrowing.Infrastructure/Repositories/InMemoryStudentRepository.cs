using EquipmentBorrowing.Application.Interfaces;

using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _students;

    public InMemoryStudentRepository(IEnumerable<Student>? students = null)
    {
        _students = students?.ToList() ?? new List<Student>();
    }

    public Task<Student?> GetByIdAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        Student? student = _students
            .FirstOrDefault(student => student.StudentId == studentId);

        return Task.FromResult(student);
    }
}
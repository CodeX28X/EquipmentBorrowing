using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _students;

    public InMemoryStudentRepository(
        IEnumerable<Student>? students = null)
    {
        _students = students?.ToList() ?? new List<Student>();
    }

    public Task<Student?> GetByIdAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        Student? student = _students
            .FirstOrDefault(
                student => student.StudentId == studentId);

        return Task.FromResult(student);
    }

    public Task<IReadOnlyList<Student>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Student> students = _students.ToList();

        return Task.FromResult(students);
    }
}
namespace EquipmentBorrowing.Domain.Entities;

public class Student
{
    public int StudentId { get; }
    public string StudentName { get; }
    public int StudentYear { get; }
    public bool IsAllowedToBorrow { get; private set; }

    public Student(
        int studentId,
        string studentName,
        int studentYear,
        bool isAllowedToBorrow)
    {
        if (studentId <= 0)
            throw new ArgumentException("Student ID must be greater than zero.", nameof(studentId));

        if (string.IsNullOrWhiteSpace(studentName))
            throw new ArgumentException("Student name is required.", nameof(studentName));

        if (studentYear <= 0)
            throw new ArgumentException("Student year must be greater than zero.", nameof(studentYear));

        StudentId = studentId;
        StudentName = studentName;
        StudentYear = studentYear;
        IsAllowedToBorrow = isAllowedToBorrow;
    }
}
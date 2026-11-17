
namespace EquipmentBorrowing.Domain.Entities;

public class Student
{
    public int StudentId { get; private set; }

    public string StudentNumber { get; private set; } = string.Empty;

    public string StudentName { get; private set; } = string.Empty;

    public int? StudentYear { get; private set; }

    public string? ContactInformation { get; private set; }

    public bool IsAllowedToBorrow { get; private set; }

    // Required by EF Core when materializing entities.
    private Student()
    {
    }

    public Student(
        int studentId,
        string studentNumber,
        string studentName,
        string? contactInformation = null,
        int? studentYear = null)
    {
        if (studentId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(studentId),
                "Student ID must be greater than zero.");
        }

        StudentId = studentId;

        UpdateInformation(
            studentNumber,
            studentName,
            contactInformation);

        if (studentYear is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(studentYear),
                "Student year must be greater than zero when supplied.");
        }

        StudentYear = studentYear;

        // BR-02: Newly registered students are eligible.
        IsAllowedToBorrow = true;
    }

    public void UpdateInformation(
        string studentNumber,
        string studentName,
        string? contactInformation)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            throw new ArgumentException(
                "Student number is required.",
                nameof(studentNumber));
        }

        if (string.IsNullOrWhiteSpace(studentName))
        {
            throw new ArgumentException(
                "Student name is required.",
                nameof(studentName));
        }

        // BR-01: Trim institutional identifiers.
        StudentNumber = studentNumber.Trim();

        // BR-02: Store a nonblank full name.
        StudentName = studentName.Trim();

        // BR-02/03: Blank optional contact means absent.
        ContactInformation =
            string.IsNullOrWhiteSpace(contactInformation)
                ? null
                : contactInformation.Trim();
    }

    public void SetBorrowingEligibility(bool isAllowed)
    {
        // BR-04: Repeating the current state is harmless.
        IsAllowedToBorrow = isAllowed;
    }
}

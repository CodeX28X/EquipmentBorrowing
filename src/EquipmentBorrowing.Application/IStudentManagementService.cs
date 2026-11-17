
namespace EquipmentBorrowing.Application;

public enum StudentOperationStatus
{
    Success,
    Rejected,
    Failed,
    OutcomeUnknown
}

public sealed record StudentOperationResult(
    StudentOperationStatus Status,
    string Message,
    int? StudentId = null)
{
    public bool IsSuccess =>
        Status == StudentOperationStatus.Success;

    public static StudentOperationResult Success(
        int studentId,
        string message) =>
        new(StudentOperationStatus.Success, message, studentId);

    public static StudentOperationResult Rejected(
        string message) =>
        new(StudentOperationStatus.Rejected, message);

    public static StudentOperationResult Failed(
        string message) =>
        new(StudentOperationStatus.Failed, message);

    public static StudentOperationResult Unknown() =>
        new(
            StudentOperationStatus.OutcomeUnknown,
            "The database may have saved the change. " +
            "Refresh student records before trying again.");
}

public interface IStudentManagementService
{
    Task<StudentOperationResult> RegisterAsync(
        string studentNumber,
        string studentName,
        string? contactInformation,
        int? studentYear,
        CancellationToken cancellationToken = default);

    Task<StudentOperationResult> UpdateAsync(
        int studentId,
        string studentNumber,
        string studentName,
        string? contactInformation,
        int? studentYear,
        CancellationToken cancellationToken = default);

    Task<StudentOperationResult> SetEligibilityAsync(
        int studentId,
        bool isAllowedToBorrow,
        CancellationToken cancellationToken = default);
}

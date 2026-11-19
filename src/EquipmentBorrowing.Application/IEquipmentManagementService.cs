
namespace EquipmentBorrowing.Application;

public enum EquipmentOperationStatus
{
    Success,
    Rejected,
    Failed,
    OutcomeUnknown
}

public sealed record EquipmentOperationResult(
    EquipmentOperationStatus Status,
    string Message,
    int? EquipmentId = null)
{
    public bool IsSuccess =>
        Status == EquipmentOperationStatus.Success;

    public static EquipmentOperationResult Success(
        int equipmentId,
        string message) =>
        new(EquipmentOperationStatus.Success, message, equipmentId);

    public static EquipmentOperationResult Rejected(
        string message) =>
        new(EquipmentOperationStatus.Rejected, message);

    public static EquipmentOperationResult Failed(
        string message) =>
        new(EquipmentOperationStatus.Failed, message);

    public static EquipmentOperationResult Unknown() =>
        new(
            EquipmentOperationStatus.OutcomeUnknown,
            "The database may have saved the change. " +
            "Refresh equipment records before trying again.");
}

public interface IEquipmentManagementService
{
    Task<EquipmentOperationResult> RegisterAsync(
        string equipmentIdentifier,
        string equipmentName,
        string? description,
        CancellationToken cancellationToken = default);

    Task<EquipmentOperationResult> UpdateAsync(
        int equipmentId,
        string equipmentIdentifier,
        string equipmentName,
        string? description,
        CancellationToken cancellationToken = default);
}

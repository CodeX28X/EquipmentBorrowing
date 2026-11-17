
namespace EquipmentBorrowing.Domain.Entities;

public class Equipment
{
    public int EquipmentId { get; private set; }

    public string EquipmentIdentifier { get; private set; }
        = string.Empty;

    public string EquipmentName { get; private set; }
        = string.Empty;

    public string? Description { get; private set; }

    public bool IsAvailable { get; private set; }

    // Required by EF Core.
    private Equipment()
    {
    }

    public Equipment(
        int equipmentId,
        string equipmentIdentifier,
        string equipmentName,
        string? description = null)
    {
        if (equipmentId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(equipmentId),
                "Equipment ID must be greater than zero.");
        }

        EquipmentId = equipmentId;

        UpdateInformation(
            equipmentIdentifier,
            equipmentName,
            description);

        // BR-06: New equipment is initially available.
        IsAvailable = true;
    }

    public void UpdateInformation(
        string equipmentIdentifier,
        string equipmentName,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(equipmentIdentifier))
        {
            throw new ArgumentException(
                "Equipment identifier is required.",
                nameof(equipmentIdentifier));
        }

        if (string.IsNullOrWhiteSpace(equipmentName))
        {
            throw new ArgumentException(
                "Equipment name is required.",
                nameof(equipmentName));
        }

        EquipmentIdentifier = equipmentIdentifier.Trim();
        EquipmentName = equipmentName.Trim();

        // BR-06: Blank optional descriptions are absent.
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    public void MarkAsBorrowed()
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException(
                "Equipment is already unavailable.");
        }

        IsAvailable = false;
    }

    public void MarkAsAvailable()
    {
        if (IsAvailable)
        {
            throw new InvalidOperationException(
                "Equipment is already available.");
        }

        IsAvailable = true;
    }
}

namespace EquipmentBorrowing.Domain.Entities;

public class Equipment
{
	public int EquipmentId { get; }
	public string EquipmentName { get; }
	public bool IsAvailable { get; private set; }

	public Equipment(
		int equipmentId,
		string equipmentName)
	{
		if (equipmentId <= 0)
			throw new ArgumentException("Equipment ID must be greater than zero.", nameof(equipmentId));

		if (string.IsNullOrWhiteSpace(equipmentName))
			throw new ArgumentException("Equipment name is required.", nameof(equipmentName));

		EquipmentId = equipmentId;
		EquipmentName = equipmentName;
		IsAvailable = true;
	}

	public void MarkAsBorrowed()
	{
		if (!IsAvailable)
			throw new InvalidOperationException("Equipment is already borrowed.");

		IsAvailable = false;
	}

	public void MarkAsAvailable()
	{
		IsAvailable = true;
	}
}
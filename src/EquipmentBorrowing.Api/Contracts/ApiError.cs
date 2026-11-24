namespace EquipmentBorrowing.Api.Contracts;

public sealed record ApiError(
    string Code,
    string Message);
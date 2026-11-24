using System.ComponentModel.DataAnnotations;

namespace EquipmentBorrowing.Api.Contracts;

public sealed record EquipmentResponse(
    int Id,
    string EquipmentCode,
    string Name,
    string? Description,
    bool IsAvailable);

public sealed class EquipmentRequest
{
    [Required]
    public required string EquipmentCode { get; init; }

    [Required]
    public required string Name { get; init; }

    public string? Description { get; init; }
}
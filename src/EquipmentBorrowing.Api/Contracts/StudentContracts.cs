using System.ComponentModel.DataAnnotations;

namespace EquipmentBorrowing.Api.Contracts;

public sealed record StudentResponse(
    int Id,
    string StudentNumber,
    string FullName,
    string? Contact,
    bool IsEligible);

public sealed class StudentRequest
{
    [Required]
    public required string StudentNumber { get; init; }

    [Required]
    public required string FullName { get; init; }

    public string? Contact { get; init; }
}

public sealed class StudentEligibilityRequest
{
    public required bool IsEligible { get; init; }
}
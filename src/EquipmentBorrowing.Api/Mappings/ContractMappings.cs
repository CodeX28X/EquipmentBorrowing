using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Api.Mappings;

public static class ContractMappings
{
    public static StudentResponse ToResponse(
        this Student student)
    {
        return new StudentResponse(
            student.StudentId,
            student.StudentNumber,
            student.StudentName,
            student.ContactInformation,
            student.IsAllowedToBorrow);
    }

    public static EquipmentResponse ToResponse(
        this Equipment equipment)
    {
        return new EquipmentResponse(
            equipment.EquipmentId,
            equipment.EquipmentIdentifier,
            equipment.EquipmentName,
            equipment.Description,
            equipment.IsAvailable);
    }
}
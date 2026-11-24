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

    public static BorrowingResponse ToResponse(
    this Borrowing borrowing)
    {
        return new BorrowingResponse(
            borrowing.BorrowingId,
            borrowing.StudentId,
            borrowing.EquipmentId,
            borrowing.BorrowedAt,
            borrowing.ExpectedReturnDate,
            borrowing.ActualReturnDate,
            borrowing.Status.ToString(),
            borrowing.Student.StudentNumber,
            borrowing.Student.StudentName,
            borrowing.Equipment.EquipmentIdentifier,
            borrowing.Equipment.EquipmentName);
    }
}
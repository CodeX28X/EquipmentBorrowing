namespace EquipmentBorrowing.Application;

public enum OperationErrorKind
{
    None,
    Validation,
    NotFound,
    Conflict,
    ServerError,
    OutcomeUnknown
}
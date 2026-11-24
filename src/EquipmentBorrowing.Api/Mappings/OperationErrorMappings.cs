using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Application;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowing.Api.Mappings;

public static class OperationErrorMappings
{
    public static ObjectResult ToErrorResponse(
        this OperationErrorKind errorKind,
        string message)
    {
        var (statusCode, code, safeMessage) =
            errorKind switch
            {
                OperationErrorKind.Validation =>
                    (400, "validation_error", message),

                OperationErrorKind.NotFound =>
                    (404, "not_found", message),

                OperationErrorKind.Conflict =>
                    (409, "conflict", message),

                OperationErrorKind.OutcomeUnknown =>
                    (500, "outcome_unknown",
                    "The operation may have been saved. " +
                    "Refresh the records before retrying."),

                _ =>
                    (500, "server_error",
                    "The server could not complete " +
                    "the request.")
            };

        return new ObjectResult(
            new ApiError(code, safeMessage))
        {
            StatusCode = statusCode
        };
    }
}
using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Api.Mappings;
using EquipmentBorrowing.Api.Services;
using EquipmentBorrowing.Application;
using Microsoft.AspNetCore.Mvc;
namespace EquipmentBorrowing.Api.Controllers;
[ApiController]
[Produces("application/json")]
[Route("api/v1/checkout-requests")]
public sealed class CheckoutRequestsController(IAtomicBorrowingService service, IBorrowingDetailsReader details, ApplicationClock clock) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CheckoutResponse), 201)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 409)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<CheckoutResponse>> Create(CheckoutRequest request, CancellationToken ct)
    {
        var today = clock.Today;
        if (request.EquipmentIds is null || request.EquipmentIds.Length < 2 || request.EquipmentIds.Distinct().Count() != request.EquipmentIds.Length || request.ExpectedReturnDate <= today)
            return BadRequest(new ApiError("validation_error", "Select at least two distinct equipment items and a future return date."));
        var result = await service.CheckoutAsync(request.StudentId, request.EquipmentIds, today, request.ExpectedReturnDate, ct);
        if (!result.IsSuccess) return result.ErrorKind.ToErrorResponse(result.Message);
        try
        {
            var items = new List<BorrowingResponse>();
            foreach (var id in result.BorrowingIds)
            {
                var item = await details.GetByIdAsync(id, ct);
                if (item is null) return OperationErrorKind.OutcomeUnknown.ToErrorResponse("Refresh borrowing records.");
                items.Add(item.ToResponse());
            }
            return StatusCode(201, new CheckoutResponse(items.ToArray()));
        }
        catch (Exception) { return OperationErrorKind.OutcomeUnknown.ToErrorResponse("Refresh borrowing records."); }
    }
}

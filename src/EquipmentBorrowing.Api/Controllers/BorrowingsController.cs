using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Api.Mappings;
using EquipmentBorrowing.Api.Services;
using EquipmentBorrowing.Application;
using Microsoft.AspNetCore.Mvc;
namespace EquipmentBorrowing.Api.Controllers;
[ApiController]
[Produces("application/json")]
[Route("api/v1/borrowings")]
public sealed class BorrowingsController(IAtomicBorrowingService service, IBorrowingDetailsReader details,
    IBorrowingSearchReader searchReader, ApplicationClock clock) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(BorrowingResponse), 201)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 409)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<BorrowingResponse>> Create(BorrowingRequest request, CancellationToken ct)
    {
        var result = await service.CheckoutAsync(request.StudentId, new[] { request.EquipmentId }, clock.Today, request.ExpectedReturnDate, ct);
        if (!result.IsSuccess) return result.ErrorKind.ToErrorResponse(result.Message);
        try
        {
            var item = await details.GetByIdAsync(result.BorrowingIds.Single(), ct);
            if (item is not null) return CreatedAtAction(nameof(GetById), new { id = item.BorrowingId }, item.ToResponse());
        }
        catch (Exception) { }
        return OperationErrorKind.OutcomeUnknown.ToErrorResponse("Refresh borrowing records.");
    }
    [HttpPost("{id}/return")]
    [ProducesResponseType(typeof(BorrowingResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 409)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<BorrowingResponse>> Return(int id, ReturnRequest request, CancellationToken ct)
    {
        var result = await service.ReturnByIdAsync(id, clock.Today, ct);
        if (!result.IsSuccess) return result.ErrorKind.ToErrorResponse(result.Message);
        try
        {
            var item = await details.GetByIdAsync(id, ct);
            if (item is not null) return Ok(item.ToResponse());
        }
        catch (Exception) { }
        return OperationErrorKind.OutcomeUnknown.ToErrorResponse("Refresh borrowing records.");
    }
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BorrowingResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<BorrowingResponse>> GetById(int id, CancellationToken ct)
    {
        var item = await details.GetByIdAsync(id, ct);
        if (item is null) return NotFound(new ApiError("not_found", "Borrowing does not exist."));
        return Ok(item.ToResponse());
    }
    [HttpGet]
    [ProducesResponseType(typeof(BorrowingResponse[]), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<BorrowingResponse[]>> GetAll(CancellationToken ct,
        [FromQuery] string? search = null, [FromQuery] string status = "All",
        [FromQuery] string sort = "borrowedDate", [FromQuery] string direction = "asc")
    {
        BorrowingSearchStatus? st = status switch { "All" => BorrowingSearchStatus.All, "Active" => BorrowingSearchStatus.Active, "Returned" => BorrowingSearchStatus.Returned, _ => null };
        BorrowingSortField? sf = sort switch { "borrowedDate" => BorrowingSortField.CheckoutDate, "expectedReturnDate" => BorrowingSortField.ExpectedReturnDate, _ => null };
        BorrowingSortDirection? sd = direction switch { "asc" => BorrowingSortDirection.Ascending, "desc" => BorrowingSortDirection.Descending, _ => null };
        if (st is null || sf is null || sd is null) return BadRequest(new ApiError("validation_error", "Unsupported status, sort or direction value."));
        var items = await searchReader.SearchAsync(new BorrowingSearchCriteria(search, st.Value, sf.Value, sd.Value), ct);
        return Ok(items.Select(b => b.ToResponse()).ToArray());
    }
}

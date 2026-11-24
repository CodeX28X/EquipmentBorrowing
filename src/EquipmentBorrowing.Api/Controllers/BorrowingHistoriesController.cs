using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Api.Mappings;
using EquipmentBorrowing.Api.Services;
using EquipmentBorrowing.Application;
using Microsoft.AspNetCore.Mvc;
namespace EquipmentBorrowing.Api.Controllers;
[ApiController]
[Produces("application/json")]
public sealed class BorrowingHistoriesController(IStudentRepository students, IEquipmentRepository equipment, IBorrowingHistoryReader history) : ControllerBase
{
    [HttpGet("api/v1/students/{id}/borrowings")]
    [ProducesResponseType(typeof(BorrowingResponse[]), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<BorrowingResponse[]>> GetStudentHistory(int id, CancellationToken ct)
    {
        if (await students.GetByIdAsync(id, ct) is null) return NotFound(new ApiError("not_found", "Student does not exist."));
        return Ok((await history.GetForStudentAsync(id, ct)).Select(b => b.ToResponse()).ToArray());
    }
    [HttpGet("api/v1/equipment/{id}/borrowings")]
    [ProducesResponseType(typeof(BorrowingResponse[]), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<BorrowingResponse[]>> GetEquipmentHistory(int id, CancellationToken ct)
    {
        if (await equipment.GetByIdAsync(id, ct) is null) return NotFound(new ApiError("not_found", "Equipment does not exist."));
        return Ok((await history.GetForEquipmentAsync(id, ct)).Select(b => b.ToResponse()).ToArray());
    }
}

using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Api.Mappings;
using EquipmentBorrowing.Application;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowing.Api.Controllers;

[ApiController]
[Route("api/v1/equipment")]
[Produces("application/json")]
public sealed class EquipmentController : ControllerBase
{
    private readonly IEquipmentRepository _equipment;

    private readonly IEquipmentManagementService _management;

    public EquipmentController(
        IEquipmentRepository equipment,
        IEquipmentManagementService management)
    {
        _equipment = equipment;
        _management = management;
    }

    // API-01
    [HttpGet]
    [ProducesResponseType(
        typeof(EquipmentResponse[]), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<EquipmentResponse[]>>
        GetAll(
            [FromQuery] bool? available,
            CancellationToken cancellationToken)
    {
        var equipment = await _equipment.GetAllAsync(
            cancellationToken);

        var results = equipment
            .Where(item =>
                !available.HasValue ||
                item.IsAvailable == available.Value)
            .Select(item => item.ToResponse())
            .ToArray();

        return Ok(results);
    }

    // API-07
    [HttpGet("{id}")]
    [ProducesResponseType(
        typeof(EquipmentResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<EquipmentResponse>>
        GetById(
            int id,
            CancellationToken cancellationToken)
    {
        var equipment =
            await _equipment.GetByIdAsync(
                id,
                cancellationToken);

        if (equipment is null)
        {
            return NotFound(
                new ApiError(
                    "not_found",
                    "Equipment does not exist."));
        }

        return Ok(equipment.ToResponse());
    }

    // API-08
    [HttpPost]
    [ProducesResponseType(
        typeof(EquipmentResponse), 201)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 409)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<EquipmentResponse>>
        Create(
            [FromBody] EquipmentRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _management.RegisterAsync(
            request.EquipmentCode,
            request.Name,
            request.Description,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorKind.ToErrorResponse(
                result.Message);
        }

        EquipmentBorrowing.Domain.Entities.Equipment? equipment;
        try
        {
            equipment = await _equipment.GetByIdAsync(
                result.EquipmentId!.Value,
                cancellationToken);
        }
        catch
        {
            return OperationErrorKind.OutcomeUnknown.ToErrorResponse("Refresh records.");
        }

        if (equipment is null)
        {
            return OperationErrorKind.OutcomeUnknown
                .ToErrorResponse(
                    "Refresh equipment records.");
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = equipment.EquipmentId },
            equipment.ToResponse());
    }

    // API-09
    [HttpPut("{id}")]
    [ProducesResponseType(
        typeof(EquipmentResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 409)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<EquipmentResponse>>
        Update(
            int id,
            [FromBody] EquipmentRequest request,
            CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return NotFound(
                new ApiError(
                    "not_found",
                    "Equipment does not exist."));
        }

        var result = await _management.UpdateAsync(
            id,
            request.EquipmentCode,
            request.Name,
            request.Description,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorKind.ToErrorResponse(
                result.Message);
        }

        EquipmentBorrowing.Domain.Entities.Equipment? equipment;
        try
        {
            equipment = await _equipment.GetByIdAsync(
                id,
                cancellationToken);
        }
        catch
        {
            return OperationErrorKind.OutcomeUnknown.ToErrorResponse("Refresh records.");
        }

        if (equipment is null)
        {
            return OperationErrorKind.OutcomeUnknown
                .ToErrorResponse(
                    "Refresh equipment records.");
        }

        return Ok(equipment.ToResponse());
    }
}
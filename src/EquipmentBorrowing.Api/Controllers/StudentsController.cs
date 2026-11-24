using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Api.Mappings;
using EquipmentBorrowing.Application;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowing.Api.Controllers;

[ApiController]
[Route("api/v1/students")]
[Produces("application/json")]
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentRepository _students;

    private readonly IStudentManagementService _management;

    public StudentsController(
        IStudentRepository students,
        IStudentManagementService management)
    {
        _students = students;
        _management = management;
    }

    // API-02
    [HttpGet]
    [ProducesResponseType(
        typeof(StudentResponse[]), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<StudentResponse[]>>
        GetAll(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        var students = await _students.GetAllAsync(
            cancellationToken);

        string? term = search?.Trim();

        var results = students
            .Where(student =>
                string.IsNullOrWhiteSpace(term) ||
                student.StudentName.Contains(
                    term,
                    StringComparison.OrdinalIgnoreCase) ||
                student.StudentNumber.Contains(
                    term,
                    StringComparison.OrdinalIgnoreCase))
            .Select(student => student.ToResponse())
            .ToArray();

        return Ok(results);
    }

    // API-03
    [HttpGet("{id}")]
    [ProducesResponseType(
        typeof(StudentResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<StudentResponse>>
        GetById(
            int id,
            CancellationToken cancellationToken)
    {
        var student = await _students.GetByIdAsync(
            id,
            cancellationToken);

        if (student is null)
        {
            return NotFound(
                new ApiError(
                    "not_found",
                    "Student does not exist."));
        }

        return Ok(student.ToResponse());
    }

    // API-04
    [HttpPost]
    [ProducesResponseType(
        typeof(StudentResponse), 201)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 409)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<StudentResponse>>
        Create(
            [FromBody] StudentRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _management.RegisterAsync(
            request.StudentNumber,
            request.FullName,
            request.Contact,
            studentYear: null,
            cancellationToken: cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorKind.ToErrorResponse(
                result.Message);
        }

        EquipmentBorrowing.Domain.Entities.Student? student;
        try
        {
            student = await _students.GetByIdAsync(
            result.StudentId!.Value,
            cancellationToken);
        }
        catch
        {
            return OperationErrorKind.OutcomeUnknown.ToErrorResponse("Refresh records.");
        }

        if (student is null)
        {
            return OperationErrorKind.OutcomeUnknown
                .ToErrorResponse(
                    "Refresh student records.");
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = student.StudentId },
            student.ToResponse());
    }

    // API-05
    [HttpPut("{id}")]
    [ProducesResponseType(
        typeof(StudentResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 409)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<StudentResponse>>
        Update(
            int id,
            [FromBody] StudentRequest request,
            CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return NotFound(
                new ApiError(
                    "not_found",
                    "Student does not exist."));
        }

        var result = await _management.UpdateProfileAsync(
            id,
            request.StudentNumber,
            request.FullName,
            request.Contact,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorKind.ToErrorResponse(
                result.Message);
        }

        EquipmentBorrowing.Domain.Entities.Student? student;
        try
        {
            student = await _students.GetByIdAsync(
            id,
            cancellationToken);
        }
        catch
        {
            return OperationErrorKind.OutcomeUnknown.ToErrorResponse("Refresh records.");
        }

        if (student is null)
        {
            return OperationErrorKind.OutcomeUnknown
                .ToErrorResponse(
                    "Refresh student records.");
        }

        return Ok(student.ToResponse());
    }

    // API-06
    [HttpPatch("{id}/eligibility")]
    [ProducesResponseType(
        typeof(StudentResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<StudentResponse>>
        SetEligibility(
            int id,
            [FromBody] StudentEligibilityRequest request,
            CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return NotFound(
                new ApiError(
                    "not_found",
                    "Student does not exist."));
        }

        var result =
            await _management.SetEligibilityAsync(
                id,
                request.IsEligible,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorKind.ToErrorResponse(
                result.Message);
        }

        EquipmentBorrowing.Domain.Entities.Student? student;
        try
        {
            student = await _students.GetByIdAsync(
            id,
            cancellationToken);
        }
        catch
        {
            return OperationErrorKind.OutcomeUnknown.ToErrorResponse("Refresh records.");
        }

        if (student is null)
        {
            return OperationErrorKind.OutcomeUnknown
                .ToErrorResponse(
                    "Refresh student records.");
        }

        return Ok(student.ToResponse());
    }
}
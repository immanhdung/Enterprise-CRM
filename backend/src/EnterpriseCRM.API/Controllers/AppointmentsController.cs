using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.TasksAndActivities;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/appointments")]
[Authorize]
public class AppointmentsController : BaseApiController
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    [HasPermission(Permissions.LeadsView)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AppointmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? dealId,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? contactId,
        [FromQuery] Guid? leadId,
        [FromQuery] string? status,
        CancellationToken cancellationToken = default)
    {
        var result = await _appointmentService.GetAppointmentsAsync(
            startDate, endDate, employeeId, dealId, accountId, contactId, leadId, status, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.LeadsView)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointmentById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.GetAppointmentByIdAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.LeadsCreate)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CreateAppointmentAsync(request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetAppointmentById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.LeadsEdit)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.UpdateAppointmentAsync(id, request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.LeadsDelete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAppointment(Guid id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.DeleteAppointmentAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }
}

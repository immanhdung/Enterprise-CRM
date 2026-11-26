using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/admin/audit-logs")]
[Authorize]
public class AuditLogsController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [HasPermission(Permissions.AuditView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AuditLogDto>>>> GetLogs([FromQuery] AuditLogFilterRequest filter, CancellationToken ct)
    {
        var logs = await _auditLogService.GetLogsAsync(filter, ct);
        return OkResponse(logs);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.AuditView)]
    public async Task<ActionResult<ApiResponse<AuditLogDetailDto>>> GetById(Guid id, CancellationToken ct)
    {
        var log = await _auditLogService.GetLogByIdAsync(id, ct);
        if (log == null) return FailResponse<AuditLogDetailDto>("Audit log not found.", StatusCodes.Status404NotFound);
        return OkResponse(log);
    }

    [HttpGet("export")]
    [HasPermission(Permissions.AuditExport)]
    public async Task<IActionResult> ExportLogs([FromQuery] AuditLogFilterRequest filter, [FromQuery] string format = "Excel", CancellationToken ct = default)
    {
        var bytes = await _auditLogService.ExportLogsAsync(filter, format, ct);
        var fileName = $"audit_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }
}

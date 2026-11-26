using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/admin/data")]
[Authorize]
public class DataManagementController : BaseApiController
{
    private readonly IDataImportExportService _dataService;
    private readonly ICurrentUserService _currentUserService;

    public DataManagementController(IDataImportExportService dataService, ICurrentUserService currentUserService)
    {
        _dataService = dataService;
        _currentUserService = currentUserService;
    }

    [HttpGet("import/jobs")]
    [HasPermission(Permissions.DataImport)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DataImportJobDto>>>> GetImportJobs(CancellationToken ct)
    {
        var jobs = await _dataService.GetImportJobsAsync(ct);
        return OkResponse(jobs);
    }

    [HttpPost("import")]
    [HasPermission(Permissions.DataImport)]
    public async Task<ActionResult<ApiResponse<ImportExecutionResultDto>>> ProcessImport([FromBody] InitiateImportRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var email = _currentUserService.Email ?? "admin";
        var result = await _dataService.ProcessImportAsync(request, userId, email, ct);
        return OkResponse(result, "Import batch processed.");
    }

    [HttpGet("export/jobs")]
    [HasPermission(Permissions.DataExport)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DataExportJobDto>>>> GetExportJobs(CancellationToken ct)
    {
        var jobs = await _dataService.GetExportJobsAsync(ct);
        return OkResponse(jobs);
    }

    [HttpPost("export")]
    [HasPermission(Permissions.DataExport)]
    public async Task<ActionResult<ApiResponse<DataExportJobDto>>> InitiateExport([FromBody] InitiateExportRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var email = _currentUserService.Email ?? "admin";
        var result = await _dataService.InitiateExportAsync(request, userId, email, ct);
        return OkResponse(result, "Export job initiated.");
    }

    [HttpGet("export/{jobId:guid}/download")]
    [HasPermission(Permissions.DataExport)]
    public async Task<IActionResult> DownloadExport(Guid jobId, CancellationToken ct)
    {
        var bytes = await _dataService.DownloadExportFileAsync(jobId, ct);
        var fileName = $"crm_export_{jobId}.csv";
        return File(bytes, "text/csv", fileName);
    }
}

using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/admin/backups")]
[Authorize]
public class BackupsController : BaseApiController
{
    private readonly IBackupAndRestoreService _backupService;
    private readonly ICurrentUserService _currentUserService;

    public BackupsController(IBackupAndRestoreService backupService, ICurrentUserService currentUserService)
    {
        _backupService = backupService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [HasPermission(Permissions.BackupsView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BackupRecordDto>>>> GetBackups(CancellationToken ct)
    {
        var records = await _backupService.GetBackupRecordsAsync(ct);
        return OkResponse(records);
    }

    [HttpPost]
    [HasPermission(Permissions.BackupsManage)]
    public async Task<ActionResult<ApiResponse<BackupRecordDto>>> InitiateBackup([FromBody] InitiateBackupRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var email = _currentUserService.Email ?? "admin";
        var result = await _backupService.InitiateBackupAsync(request, userId, email, ct);
        return OkResponse(result, "Verified backup snapshot created successfully.");
    }

    [HttpGet("restores")]
    [HasPermission(Permissions.BackupsView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RestoreRecordDto>>>> GetRestores(CancellationToken ct)
    {
        var records = await _backupService.GetRestoreRecordsAsync(ct);
        return OkResponse(records);
    }

    [HttpPost("restore-drill")]
    [HasPermission(Permissions.BackupsRestore)]
    public async Task<ActionResult<ApiResponse<RestoreRecordDto>>> ExecuteRestoreDrill([FromBody] ExecuteRestoreDrillRequest request, CancellationToken ct)
    {
        try
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            var email = _currentUserService.Email ?? "admin";
            var result = await _backupService.ExecuteRestoreDrillAsync(request, userId, email, ct);
            return OkResponse(result, "Non-destructive restore verification drill completed successfully.");
        }
        catch (KeyNotFoundException ex)
        {
            return FailResponse<RestoreRecordDto>(ex.Message, StatusCodes.Status404NotFound);
        }
    }

    [HttpGet("supabase-pitr")]
    [HasPermission(Permissions.BackupsView)]
    public async Task<ActionResult<ApiResponse<SupabasePitrStatusDto>>> GetSupabasePitrStatus(CancellationToken ct)
    {
        var result = await _backupService.GetSupabasePitrStatusAsync(ct);
        return OkResponse(result);
    }
}

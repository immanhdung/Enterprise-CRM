using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/admin/organization")]
[Authorize]
public class OrganizationAdministrationController : BaseApiController
{
    private readonly IOrganizationAdministrationService _orgService;
    private readonly ICurrentUserService _currentUserService;

    public OrganizationAdministrationController(IOrganizationAdministrationService orgService, ICurrentUserService currentUserService)
    {
        _orgService = orgService;
        _currentUserService = currentUserService;
    }

    [HttpGet("hierarchy")]
    [HasPermission(Permissions.DepartmentsView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DepartmentHierarchyDto>>>> GetHierarchy(CancellationToken ct)
    {
        var result = await _orgService.GetHierarchyAsync(ct);
        return OkResponse(result);
    }

    [HttpPost("transfer")]
    [HasPermission(Permissions.EmployeesEdit)]
    public async Task<ActionResult<ApiResponse<OrganizationChangeHistoryDto>>> TransferEmployee([FromBody] TransferEmployeeRequest request, CancellationToken ct)
    {
        try
        {
            var approvedBy = _currentUserService.UserId ?? Guid.Empty;
            var result = await _orgService.TransferEmployeeAsync(request, approvedBy, ct);
            return OkResponse(result, "Employee transferred successfully.");
        }
        catch (KeyNotFoundException ex)
        {
            return FailResponse<OrganizationChangeHistoryDto>(ex.Message, StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            return FailResponse<OrganizationChangeHistoryDto>(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpGet("history")]
    [HasPermission(Permissions.EmployeesView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrganizationChangeHistoryDto>>>> GetHistory([FromQuery] Guid? employeeId, CancellationToken ct)
    {
        var result = await _orgService.GetChangeHistoryAsync(employeeId, ct);
        return OkResponse(result);
    }
}

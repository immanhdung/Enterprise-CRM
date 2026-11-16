using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/contract-templates")]
public class ContractTemplatesController : BaseApiController
{
    private readonly IContractTemplateService _templateService;

    public ContractTemplatesController(IContractTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ContractTemplateDto>>>> GetTemplates([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetTemplatesAsync(includeInactive, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ContractTemplateDto>>> GetTemplateById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetTemplateByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(ApiResponse<ContractTemplateDto>.Failed("Contract template not found"));
        return OkResponse(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ContractTemplateDto>>> CreateTemplate([FromBody] CreateContractTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.CreateTemplateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTemplateById), new { id = result.Id }, ApiResponse<ContractTemplateDto>.Succeeded(result, "Contract template created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ContractTemplateDto>>> UpdateTemplate(Guid id, [FromBody] UpdateContractTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.UpdateTemplateAsync(id, request, cancellationToken);
        return OkResponse(result, "Contract template updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTemplate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _templateService.DeleteTemplateAsync(id, cancellationToken);
        if (!result) return NotFound(ApiResponse<bool>.Failed("Contract template not found"));
        return OkResponse(true, "Contract template deleted successfully.");
    }
}

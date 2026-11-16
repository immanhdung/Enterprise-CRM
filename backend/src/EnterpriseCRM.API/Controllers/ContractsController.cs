using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
public class ContractsController : BaseApiController
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedContractResult>>> GetContracts([FromQuery] ContractFilterParams filter, CancellationToken cancellationToken = default)
    {
        var result = await _contractService.GetContractsAsync(filter, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> GetContractById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _contractService.GetContractByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(ApiResponse<ContractDto>.Failed("Contract not found"));
        return OkResponse(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ContractDto>>> CreateContract([FromBody] CreateContractRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _contractService.CreateContractAsync(request, cancellationToken);
        return OkResponse(result, "Contract created successfully.");
    }

    [HttpPost("from-quotation")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> GenerateFromQuotation([FromBody] GenerateContractFromQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _contractService.GenerateContractFromQuotationAsync(request, cancellationToken);
        return OkResponse(result, "Contract generated from quotation successfully.");
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> UpdateContract(Guid id, [FromBody] UpdateContractRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _contractService.UpdateContractAsync(id, request, cancellationToken);
        return OkResponse(result, "Contract updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteContract(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _contractService.DeleteContractAsync(id, cancellationToken);
        if (!result) return NotFound(ApiResponse<bool>.Failed("Contract not found"));
        return OkResponse(true, "Contract deleted successfully.");
    }

    [HttpGet("{id:guid}/content")]
    public async Task<ActionResult<ApiResponse<string>>> RenderContractContent(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _contractService.RenderContractContentAsync(id, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadContractPdf(Guid id, CancellationToken cancellationToken = default)
    {
        var pdfBytes = await _contractService.GenerateContractPdfAsync(id, cancellationToken);
        return File(pdfBytes, "application/pdf", $"contract-{id}.pdf");
    }

    [HttpGet("{id:guid}/appendices")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ContractAppendixDto>>>> GetAppendices(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _contractService.GetAppendicesAsync(id, cancellationToken);
        return OkResponse(result);
    }

    [HttpPost("{id:guid}/appendices")]
    public async Task<ActionResult<ApiResponse<ContractAppendixDto>>> CreateAppendix(Guid id, [FromBody] CreateContractAppendixRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _contractService.CreateAppendixAsync(id, request, cancellationToken);
        return OkResponse(result, "Contract appendix created successfully.");
    }

    [HttpPost("{id:guid}/renew")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> RenewContract(Guid id, [FromBody] RenewContractRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _contractService.RenewContractAsync(id, request, cancellationToken);
        return OkResponse(result, "Contract renewed successfully.");
    }
}

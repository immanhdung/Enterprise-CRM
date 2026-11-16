using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Quotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
public class QuotationsController : BaseApiController
{
    private readonly IQuotationService _quotationService;

    public QuotationsController(IQuotationService quotationService)
    {
        _quotationService = quotationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedQuotationResult>>> GetQuotations([FromQuery] QuotationFilterParams filter, CancellationToken cancellationToken = default)
    {
        var result = await _quotationService.GetQuotationsAsync(filter, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<QuotationDto>>> GetQuotationById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _quotationService.GetQuotationByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(ApiResponse<QuotationDto>.Failed("Quotation not found"));
        return OkResponse(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<QuotationDto>>> CreateQuotation([FromBody] CreateQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _quotationService.CreateQuotationAsync(request, cancellationToken);
        return OkResponse(result, "Quotation created successfully.");
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<QuotationDto>>> UpdateQuotation(Guid id, [FromBody] UpdateQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _quotationService.UpdateQuotationAsync(id, request, cancellationToken);
        return OkResponse(result, "Quotation updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteQuotation(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _quotationService.DeleteQuotationAsync(id, cancellationToken);
        if (!result) return NotFound(ApiResponse<bool>.Failed("Quotation not found"));
        return OkResponse(true, "Quotation deleted successfully.");
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<ApiResponse<QuotationDto>>> SubmitForApproval(Guid id, [FromBody] SubmitQuotationApprovalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _quotationService.SubmitForApprovalAsync(id, request, cancellationToken);
        return OkResponse(result, "Quotation submitted for approval.");
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<QuotationDto>>> ApproveQuotation(Guid id, [FromBody] ApproveQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _quotationService.ApproveQuotationAsync(id, request, cancellationToken);
        return OkResponse(result, "Quotation approved successfully.");
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ApiResponse<QuotationDto>>> RejectQuotation(Guid id, [FromBody] RejectQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _quotationService.RejectQuotationAsync(id, request, cancellationToken);
        return OkResponse(result, "Quotation rejected.");
    }

    [HttpPost("{id:guid}/revise")]
    public async Task<ActionResult<ApiResponse<QuotationDto>>> CreateRevision(Guid id, [FromBody] CreateRevisionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _quotationService.CreateRevisionAsync(id, request, cancellationToken);
        return OkResponse(result, $"New revision {result.RevisionNumber} created successfully.");
    }

    [HttpGet("{id:guid}/versions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuotationDto>>>> GetQuotationVersions(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _quotationService.GetQuotationVersionsAsync(id, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadQuotationPdf(Guid id, CancellationToken cancellationToken = default)
    {
        var pdfBytes = await _quotationService.GenerateQuotationPdfAsync(id, cancellationToken);
        return File(pdfBytes, "application/pdf", $"quotation-{id}.pdf");
    }

    [HttpPost("{id:guid}/send-email")]
    public async Task<ActionResult<ApiResponse<bool>>> SendQuotationEmail(Guid id, [FromBody] SendQuotationEmailRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _quotationService.SendQuotationEmailAsync(id, request, cancellationToken);
        return OkResponse(result, "Quotation email sent to customer.");
    }
}

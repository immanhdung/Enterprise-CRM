using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Application.Features.Quotations;

public interface IQuotationService
{
    Task<PagedQuotationResult> GetQuotationsAsync(QuotationFilterParams filter, CancellationToken cancellationToken = default);
    Task<QuotationDto?> GetQuotationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<QuotationDto> CreateQuotationAsync(CreateQuotationRequest request, CancellationToken cancellationToken = default);
    Task<QuotationDto> UpdateQuotationAsync(Guid id, UpdateQuotationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteQuotationAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Approval Workflow
    Task<QuotationDto> SubmitForApprovalAsync(Guid id, SubmitQuotationApprovalRequest request, CancellationToken cancellationToken = default);
    Task<QuotationDto> ApproveQuotationAsync(Guid id, ApproveQuotationRequest request, CancellationToken cancellationToken = default);
    Task<QuotationDto> RejectQuotationAsync(Guid id, RejectQuotationRequest request, CancellationToken cancellationToken = default);

    // Versioning / Revisions
    Task<QuotationDto> CreateRevisionAsync(Guid id, CreateRevisionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuotationDto>> GetQuotationVersionsAsync(Guid id, CancellationToken cancellationToken = default);

    // PDF and Email
    Task<byte[]> GenerateQuotationPdfAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SendQuotationEmailAsync(Guid id, SendQuotationEmailRequest request, CancellationToken cancellationToken = default);
}

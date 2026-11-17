namespace EnterpriseCRM.Application.Features.Finance;

public interface IPaymentService
{
    Task<PagedPaymentResult> GetPaymentsAsync(PaymentFilterParams filter, CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default);
    Task<PaymentDto> ConfirmPaymentAsync(Guid id, ConfirmPaymentRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default);
    Task<PaymentDto> ReversePaymentAsync(Guid id, ReversePaymentRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default);
    Task<ContractFinancialSummaryDto> GetContractFinancialSummaryAsync(Guid contractId, CancellationToken cancellationToken = default);
}

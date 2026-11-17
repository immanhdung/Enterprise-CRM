namespace EnterpriseCRM.Application.Features.Finance;

public interface IPaymentScheduleService
{
    Task<IReadOnlyList<PaymentScheduleDto>> GetContractSchedulesAsync(Guid contractId, CancellationToken cancellationToken = default);
    Task<PaymentScheduleDto> CreateScheduleAsync(Guid contractId, CreatePaymentScheduleRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentScheduleDto>> GenerateContractSchedulesAsync(Guid contractId, GenerateInstallmentSchedulesRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default);
}

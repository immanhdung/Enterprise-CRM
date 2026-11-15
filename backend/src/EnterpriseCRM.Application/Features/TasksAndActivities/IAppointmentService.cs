using EnterpriseCRM.Application.Common.Models;

namespace EnterpriseCRM.Application.Features.TasksAndActivities;

public interface IAppointmentService
{
    Task<ApiResponse<IReadOnlyList<AppointmentDto>>> GetAppointmentsAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? employeeId,
        Guid? dealId,
        Guid? accountId,
        Guid? contactId,
        Guid? leadId,
        string? status,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<AppointmentDto>> GetAppointmentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<AppointmentDto>> UpdateAppointmentAsync(Guid id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteAppointmentAsync(Guid id, CancellationToken cancellationToken = default);
}

using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.TasksAndActivities;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AppointmentService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<AppointmentDto>>> GetAppointmentsAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? employeeId,
        Guid? dealId,
        Guid? accountId,
        Guid? contactId,
        Guid? leadId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .Include(a => a.Employee)
                .ThenInclude(e => e.User)
            .Include(a => a.Deal)
            .Include(a => a.Account)
            .Include(a => a.Contact)
            .Include(a => a.Lead)
            .AsQueryable();

        if (startDate.HasValue) query = query.Where(a => a.EndTime >= startDate.Value);
        if (endDate.HasValue) query = query.Where(a => a.StartTime <= endDate.Value);
        if (employeeId.HasValue) query = query.Where(a => a.EmployeeId == employeeId.Value);
        if (dealId.HasValue) query = query.Where(a => a.DealId == dealId.Value);
        if (accountId.HasValue) query = query.Where(a => a.AccountId == accountId.Value);
        if (contactId.HasValue) query = query.Where(a => a.ContactId == contactId.Value);
        if (leadId.HasValue) query = query.Where(a => a.LeadId == leadId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);

        var items = await query
            .OrderBy(a => a.StartTime)
            .Select(a => new AppointmentDto(
                a.Id,
                a.Title,
                a.Description,
                a.StartTime,
                a.EndTime,
                a.Location,
                a.MeetingLink,
                a.Status,
                a.EmployeeId,
                a.Employee.User.FullName,
                a.DealId,
                a.Deal != null ? a.Deal.Title : null,
                a.AccountId,
                a.Account != null ? a.Account.Name : null,
                a.ContactId,
                a.Contact != null ? a.Contact.FullName : null,
                a.LeadId,
                a.Lead != null ? a.Lead.FullName : null,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<AppointmentDto>>.SuccessResponse(items);
    }

    public async Task<ApiResponse<AppointmentDto>> GetAppointmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var a = await _context.Appointments
            .Include(x => x.Employee)
                .ThenInclude(e => e.User)
            .Include(x => x.Deal)
            .Include(x => x.Account)
            .Include(x => x.Contact)
            .Include(x => x.Lead)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (a == null)
        {
            return ApiResponse<AppointmentDto>.FailureResponse("Appointment not found.", 404);
        }

        var dto = new AppointmentDto(
            a.Id,
            a.Title,
            a.Description,
            a.StartTime,
            a.EndTime,
            a.Location,
            a.MeetingLink,
            a.Status,
            a.EmployeeId,
            a.Employee.User.FullName,
            a.DealId,
            a.Deal?.Title,
            a.AccountId,
            a.Account?.Name,
            a.ContactId,
            a.Contact?.FullName,
            a.LeadId,
            a.Lead?.FullName,
            a.CreatedAt,
            a.UpdatedAt
        );

        return ApiResponse<AppointmentDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        // Resolve EmployeeId safely
        var employeeExists = request.EmployeeId != Guid.Empty &&
                             await _context.Employees.AnyAsync(e => e.Id == request.EmployeeId, cancellationToken);

        Guid resolvedEmployeeId;
        if (employeeExists)
        {
            resolvedEmployeeId = request.EmployeeId;
        }
        else
        {
            // Try resolving by current user
            var currentEmp = _currentUserService.UserId.HasValue
                ? await _context.Employees.FirstOrDefaultAsync(e => e.UserId == _currentUserService.UserId.Value, cancellationToken)
                : null;

            if (currentEmp != null)
            {
                resolvedEmployeeId = currentEmp.Id;
            }
            else
            {
                var fallbackEmpId = await _context.Employees.Select(e => e.Id).FirstOrDefaultAsync(cancellationToken);
                if (fallbackEmpId == Guid.Empty)
                {
                    return ApiResponse<AppointmentDto>.FailureResponse("No active employee found to assign appointment.", 400);
                }
                resolvedEmployeeId = fallbackEmpId;
            }
        }

        var appt = new Appointment
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Location = request.Location?.Trim(),
            MeetingLink = request.MeetingLink?.Trim(),
            EmployeeId = resolvedEmployeeId,
            DealId = request.DealId,
            AccountId = request.AccountId,
            ContactId = request.ContactId,
            LeadId = request.LeadId,
            Status = "Scheduled"
        };

        _context.Appointments.Add(appt);

        // Record in Timeline
        _context.CrmActivities.Add(new CrmActivity
        {
            Id = Guid.NewGuid(),
            LeadId = request.LeadId,
            AccountId = request.AccountId,
            ContactId = request.ContactId,
            DealId = request.DealId,
            ActivityType = "Meeting",
            Subject = $"Meeting Scheduled: '{appt.Title}'",
            Notes = $"From {appt.StartTime:yyyy-MM-dd HH:mm} to {appt.EndTime:HH:mm}. Location: {appt.Location ?? appt.MeetingLink ?? "TBD"}",
            PerformedByUserId = _currentUserService.UserId,
            PerformedByName = _currentUserService.Email ?? "User",
            ScheduledAt = appt.StartTime,
            CompletedAt = null
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created Appointment {AppointmentId} ('{Title}')", appt.Id, appt.Title);

        return await GetAppointmentByIdAsync(appt.Id, cancellationToken);
    }

    public async Task<ApiResponse<AppointmentDto>> UpdateAppointmentAsync(Guid id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (appt == null)
        {
            return ApiResponse<AppointmentDto>.FailureResponse("Appointment not found.", 404);
        }

        appt.Title = request.Title.Trim();
        appt.Description = request.Description?.Trim();
        appt.StartTime = request.StartTime;
        appt.EndTime = request.EndTime;
        appt.Location = request.Location?.Trim();
        appt.MeetingLink = request.MeetingLink?.Trim();
        appt.Status = request.Status;
        appt.EmployeeId = request.EmployeeId;
        appt.DealId = request.DealId;
        appt.AccountId = request.AccountId;
        appt.ContactId = request.ContactId;
        appt.LeadId = request.LeadId;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetAppointmentByIdAsync(appt.Id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteAppointmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (appt == null)
        {
            return ApiResponse<bool>.FailureResponse("Appointment not found.", 404);
        }

        appt.IsDeleted = true;
        appt.DeletedAt = DateTime.UtcNow;
        appt.DeletedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Appointment deleted successfully.");
    }
}

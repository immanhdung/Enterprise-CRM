using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.TasksAndActivities;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/timeline")]
[Authorize]
public class TimelineController : BaseApiController
{
    private readonly ITimelineService _timelineService;

    public TimelineController(ITimelineService timelineService)
    {
        _timelineService = timelineService;
    }

    [HttpGet]
    [HasPermission(Permissions.LeadsView)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TimelineEventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeline(
        [FromQuery] Guid? leadId,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? contactId,
        [FromQuery] Guid? dealId,
        [FromQuery] string? activityType,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _timelineService.GetTimelineAsync(leadId, accountId, contactId, dealId, activityType, limit, cancellationToken);
        return Ok(result);
    }

    public record RecordActivityRequest(
        string ActivityType,
        string Subject,
        string? Notes,
        Guid? LeadId,
        Guid? AccountId,
        Guid? ContactId,
        Guid? DealId,
        DateTime? ScheduledAt,
        DateTime? CompletedAt
    );

    [HttpPost]
    [HasPermission(Permissions.LeadsEdit)]
    [ProducesResponseType(typeof(ApiResponse<TimelineEventDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> RecordActivity([FromBody] RecordActivityRequest request, CancellationToken cancellationToken)
    {
        var result = await _timelineService.RecordActivityAsync(
            request.ActivityType,
            request.Subject,
            request.Notes,
            request.LeadId,
            request.AccountId,
            request.ContactId,
            request.DealId,
            request.ScheduledAt,
            request.CompletedAt,
            cancellationToken);

        return Ok(result);
    }
}

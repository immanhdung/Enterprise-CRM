using EnterpriseCRM.Application.Common.Interfaces;

namespace EnterpriseCRM.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}

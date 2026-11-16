using EnterpriseCRM.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        byte[]? attachmentBytes = null,
        string? attachmentFileName = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending email to {ToEmail} with Subject: '{Subject}'. Attachment: {AttachmentFileName} ({Bytes} bytes)",
            toEmail, subject, attachmentFileName ?? "none", attachmentBytes?.Length ?? 0);

        // In production, wire to SendGrid, AWS SES, or SMTP.
        // For test/integration, we log and return success.
        return Task.FromResult(true);
    }
}

using FluentValidation;

namespace EnterpriseCRM.Application.Features.AutomationAndAi;

public class CreateWorkflowDefinitionRequestValidator : AbstractValidator<CreateWorkflowDefinitionRequest>
{
    public CreateWorkflowDefinitionRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TriggerEvent).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ExecutionOrder).GreaterThanOrEqualTo(1);
    }
}

public class UpdateWorkflowDefinitionRequestValidator : AbstractValidator<UpdateWorkflowDefinitionRequest>
{
    public UpdateWorkflowDefinitionRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TriggerEvent).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ExecutionOrder).GreaterThanOrEqualTo(1);
    }
}

public class CreateLeadAssignmentRuleRequestValidator : AbstractValidator<CreateLeadAssignmentRuleRequest>
{
    public CreateLeadAssignmentRuleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Mode).NotEmpty().Must(m => new[] { "RoundRobin", "Workload", "Territory", "Source", "ServiceSpecialization" }.Contains(m))
            .WithMessage("Invalid assignment mode.");
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(1);
    }
}

public class CreateAlertRuleRequestValidator : AbstractValidator<CreateAlertRuleRequest>
{
    public CreateAlertRuleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AlertType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ThresholdDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Severity).Must(s => new[] { "Info", "Warning", "Critical" }.Contains(s))
            .WithMessage("Severity must be Info, Warning, or Critical.");
    }
}

public class AiChatRequestValidator : AbstractValidator<AiChatRequest>
{
    public AiChatRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
    }
}

public class BusinessQueryRequestValidator : AbstractValidator<BusinessQueryRequest>
{
    public BusinessQueryRequestValidator()
    {
        RuleFor(x => x.Query).NotEmpty().MaximumLength(1000);
    }
}

public class CreateScheduledReportRequestValidator : AbstractValidator<CreateScheduledReportRequest>
{
    public CreateScheduledReportRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReportType).Must(t => new[] { "Daily", "Weekly", "Monthly", "AdHoc" }.Contains(t))
            .WithMessage("ReportType must be Daily, Weekly, Monthly, or AdHoc.");
        RuleFor(x => x.Format).Must(f => new[] { "PDF", "Excel", "InApp" }.Contains(f))
            .WithMessage("Format must be PDF, Excel, or InApp.");
        RuleFor(x => x.TimeOfDay).NotEmpty().Matches(@"^([01]\d|2[0-3]):[0-5]\d$")
            .WithMessage("TimeOfDay must be in HH:mm format.");
    }
}

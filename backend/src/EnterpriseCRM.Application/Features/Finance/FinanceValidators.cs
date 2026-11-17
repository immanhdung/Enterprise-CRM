using FluentValidation;

namespace EnterpriseCRM.Application.Features.Finance;

public class CreatePaymentScheduleRequestValidator : AbstractValidator<CreatePaymentScheduleRequest>
{
    public CreatePaymentScheduleRequestValidator()
    {
        RuleFor(x => x.MilestoneTitle)
            .NotEmpty().WithMessage("Milestone title is required.")
            .MaximumLength(200);

        RuleFor(x => x.InstallmentNumber)
            .GreaterThan(0).WithMessage("Installment number must be greater than 0.");

        RuleFor(x => x.Percentage)
            .GreaterThan(0).WithMessage("Percentage must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Percentage cannot exceed 100.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");
    }
}

public class GenerateInstallmentSchedulesRequestValidator : AbstractValidator<GenerateInstallmentSchedulesRequest>
{
    public GenerateInstallmentSchedulesRequestValidator()
    {
        RuleFor(x => x.Installments)
            .NotEmpty().WithMessage("At least one installment is required.")
            .Must(list =>
            {
                if (list == null || list.Count == 0) return false;
                var totalPercentage = list.Sum(i => i.Percentage);
                return Math.Abs(totalPercentage - 100m) <= 0.01m;
            }).WithMessage("The sum of all installment percentages must equal exactly 100%.");

        RuleForEach(x => x.Installments).ChildRules(item =>
        {
            item.RuleFor(i => i.MilestoneTitle).NotEmpty().WithMessage("Milestone title is required.");
            item.RuleFor(i => i.Percentage).GreaterThan(0).WithMessage("Percentage must be greater than 0.");
        });
    }
}

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEmpty().WithMessage("Contract ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required.");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Invalid payment method.");
    }
}

public class ReversePaymentRequestValidator : AbstractValidator<ReversePaymentRequest>
{
    public ReversePaymentRequestValidator()
    {
        RuleFor(x => x.ReversalReason)
            .NotEmpty().WithMessage("Reversal reason is mandatory for financial audit.")
            .MinimumLength(5).WithMessage("Reversal reason must be at least 5 characters.")
            .MaximumLength(500);
    }
}

public class IssueInvoiceRequestValidator : AbstractValidator<IssueInvoiceRequest>
{
    public IssueInvoiceRequestValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEmpty().WithMessage("Contract ID is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one invoice line item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).NotEmpty().WithMessage("Item description is required.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
            item.RuleFor(i => i.TaxRate).GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative.");
        });
    }
}

public class CancelInvoiceRequestValidator : AbstractValidator<CancelInvoiceRequest>
{
    public CancelInvoiceRequestValidator()
    {
        RuleFor(x => x.CancellationReason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MinimumLength(5).WithMessage("Cancellation reason must be at least 5 characters.")
            .MaximumLength(500);
    }
}

public class CreateInvoiceRequestInputValidator : AbstractValidator<CreateInvoiceRequestInput>
{
    public CreateInvoiceRequestInputValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEmpty().WithMessage("Contract ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Requested invoice amount must be greater than 0.");
    }
}

public class CreateReceivableCollectionLogRequestValidator : AbstractValidator<CreateReceivableCollectionLogRequest>
{
    public CreateReceivableCollectionLogRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account ID is required.");

        RuleFor(x => x.ContractId)
            .NotEmpty().WithMessage("Contract ID is required.");

        RuleFor(x => x.ActivityType)
            .NotEmpty().WithMessage("Activity type is required.");

        RuleFor(x => x.Notes)
            .NotEmpty().WithMessage("Activity notes/summary is required.");
    }
}

public class RunReconciliationRequestValidator : AbstractValidator<RunReconciliationRequest>
{
    public RunReconciliationRequestValidator()
    {
        RuleFor(x => x.PeriodStart)
            .NotEmpty().WithMessage("Period start is required.");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty().WithMessage("Period end is required.")
            .GreaterThanOrEqualTo(x => x.PeriodStart).WithMessage("Period end must be greater than or equal to period start.");
    }
}

public class ResolveDiscrepancyRequestValidator : AbstractValidator<ResolveDiscrepancyRequest>
{
    public ResolveDiscrepancyRequestValidator()
    {
        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .Must(a => a == "ConfirmAdjustment" || a == "Dismiss")
            .WithMessage("Action must be either 'ConfirmAdjustment' or 'Dismiss'.");

        RuleFor(x => x.ResolutionNotes)
            .NotEmpty().WithMessage("Resolution notes are required for audit trail.");
    }
}

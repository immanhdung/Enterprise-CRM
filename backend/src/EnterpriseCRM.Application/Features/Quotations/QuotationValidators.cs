using FluentValidation;

namespace EnterpriseCRM.Application.Features.Quotations;

public class CreateQuotationRequestValidator : AbstractValidator<CreateQuotationRequest>
{
    public CreateQuotationRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account is required.");

        RuleFor(x => x.ValidUntil)
            .GreaterThan(DateTime.UtcNow.Date.AddDays(-1)).WithMessage("Valid until date must not be in the past.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Quotation must contain at least one line item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ServiceName).NotEmpty().WithMessage("Service name is required.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Unit price must be >= 0.");
            item.RuleFor(i => i.DiscountRate).InclusiveBetween(0, 100).WithMessage("Discount rate must be between 0 and 100.");
            item.RuleFor(i => i.VatRate).InclusiveBetween(0, 100).WithMessage("VAT rate must be between 0 and 100.");
        });
    }
}

public class UpdateQuotationRequestValidator : AbstractValidator<UpdateQuotationRequest>
{
    public UpdateQuotationRequestValidator()
    {
        RuleFor(x => x.ValidUntil)
            .GreaterThan(DateTime.UtcNow.Date.AddDays(-1)).WithMessage("Valid until date must not be in the past.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Quotation must contain at least one line item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ServiceName).NotEmpty().WithMessage("Service name is required.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Unit price must be >= 0.");
            item.RuleFor(i => i.DiscountRate).InclusiveBetween(0, 100).WithMessage("Discount rate must be between 0 and 100.");
            item.RuleFor(i => i.VatRate).InclusiveBetween(0, 100).WithMessage("VAT rate must be between 0 and 100.");
        });
    }
}

public class RejectQuotationRequestValidator : AbstractValidator<RejectQuotationRequest>
{
    public RejectQuotationRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters.");
    }
}

public class SendQuotationEmailRequestValidator : AbstractValidator<SendQuotationEmailRequest>
{
    public SendQuotationEmailRequestValidator()
    {
        RuleFor(x => x.RecipientEmail)
            .NotEmpty().WithMessage("Recipient email is required.")
            .EmailAddress().WithMessage("Invalid recipient email address.");
    }
}

using FluentValidation;

namespace EnterpriseCRM.Application.Features.Leads;

public class CreateLeadRequestValidator : AbstractValidator<CreateLeadRequest>
{
    public CreateLeadRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^[+0-9\s\-()]{7,20}$").WithMessage("Valid phone number is required (7-20 digits/symbols).");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.EstimatedBudget)
            .GreaterThanOrEqualTo(0).WithMessage("Estimated budget cannot be negative.");
    }
}

public class UpdateLeadRequestValidator : AbstractValidator<UpdateLeadRequest>
{
    public UpdateLeadRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.");

        RuleFor(x => x.EstimatedBudget)
            .GreaterThanOrEqualTo(0).WithMessage("Estimated budget cannot be negative.");
    }
}

public class ConvertLeadRequestValidator : AbstractValidator<ConvertLeadRequest>
{
    public ConvertLeadRequestValidator()
    {
        RuleFor(x => x.DealTitle)
            .NotEmpty().WithMessage("Deal title is required for lead conversion.");

        RuleFor(x => x.DealAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Deal amount cannot be negative.");
    }
}

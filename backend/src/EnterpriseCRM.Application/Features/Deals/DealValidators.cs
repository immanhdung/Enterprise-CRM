using FluentValidation;

namespace EnterpriseCRM.Application.Features.Deals;

public class CreateDealRequestValidator : AbstractValidator<CreateDealRequest>
{
    public CreateDealRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Deal title is required.")
            .MaximumLength(200);

        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account association is required.");

        RuleFor(x => x.StageId)
            .NotEmpty().WithMessage("Pipeline stage is required.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Deal amount cannot be negative.");

        RuleFor(x => x.ProbabilityOverride)
            .InclusiveBetween(0, 100).When(x => x.ProbabilityOverride.HasValue)
            .WithMessage("Probability must be between 0 and 100%.");
    }
}

public class UpdateDealRequestValidator : AbstractValidator<UpdateDealRequest>
{
    public UpdateDealRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Deal title is required.")
            .MaximumLength(200);

        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account association is required.");

        RuleFor(x => x.StageId)
            .NotEmpty().WithMessage("Pipeline stage is required.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Deal amount cannot be negative.");

        RuleFor(x => x.ProbabilityOverride)
            .InclusiveBetween(0, 100).When(x => x.ProbabilityOverride.HasValue)
            .WithMessage("Probability must be between 0 and 100%.");
    }
}

public class UpdateDealStageRequestValidator : AbstractValidator<UpdateDealStageRequest>
{
    public UpdateDealStageRequestValidator()
    {
        RuleFor(x => x.NewStageId)
            .NotEmpty().WithMessage("Target pipeline stage is required.");
    }
}

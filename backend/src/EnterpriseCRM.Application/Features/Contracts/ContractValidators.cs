using FluentValidation;

namespace EnterpriseCRM.Application.Features.Contracts;

public class CreateContractRequestValidator : AbstractValidator<CreateContractRequest>
{
    public CreateContractRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Contract title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => x.TotalValue)
            .GreaterThanOrEqualTo(0).WithMessage("Total value must be greater than or equal to 0.");
    }
}

public class GenerateContractFromQuotationRequestValidator : AbstractValidator<GenerateContractFromQuotationRequest>
{
    public GenerateContractFromQuotationRequestValidator()
    {
        RuleFor(x => x.QuotationId)
            .NotEmpty().WithMessage("Quotation is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");
    }
}

public class UpdateContractRequestValidator : AbstractValidator<UpdateContractRequest>
{
    public UpdateContractRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Contract title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => x.TotalValue)
            .GreaterThanOrEqualTo(0).WithMessage("Total value must be greater than or equal to 0.");
    }
}

public class CreateContractAppendixRequestValidator : AbstractValidator<CreateContractAppendixRequest>
{
    public CreateContractAppendixRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Appendix title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
    }
}

public class RenewContractRequestValidator : AbstractValidator<RenewContractRequest>
{
    public RenewContractRequestValidator()
    {
        RuleFor(x => x.NewStartDate)
            .NotEmpty().WithMessage("New start date is required.");

        RuleFor(x => x.NewEndDate)
            .GreaterThan(x => x.NewStartDate).WithMessage("New end date must be after new start date.");
    }
}

public class CreateContractTemplateRequestValidator : AbstractValidator<CreateContractTemplateRequest>
{
    public CreateContractTemplateRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Template code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template name is required.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Template content is required.");
    }
}

public class UpdateContractTemplateRequestValidator : AbstractValidator<UpdateContractTemplateRequest>
{
    public UpdateContractTemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template name is required.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Template content is required.");
    }
}

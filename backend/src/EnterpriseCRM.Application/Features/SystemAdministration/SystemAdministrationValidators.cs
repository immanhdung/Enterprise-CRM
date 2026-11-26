using FluentValidation;

namespace EnterpriseCRM.Application.Features.SystemAdministration;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email address is required.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage("First name is required.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage("Last name is required.");
        RuleFor(x => x.Roles).NotEmpty().WithMessage("At least one role must be assigned.");
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage("First name is required.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage("Last name is required.");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required.");
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).WithMessage("New password must be at least 6 characters.");
    }
}

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Role name is required.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(250).WithMessage("Role description is required.");
    }
}

public class CreateWebhookEndpointRequestValidator : AbstractValidator<CreateWebhookEndpointRequest>
{
    public CreateWebhookEndpointRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("Endpoint name is required.");
        RuleFor(x => x.Url).NotEmpty().Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).WithMessage("A valid absolute URL is required.");
        RuleFor(x => x.SubscribedEvents).NotEmpty().WithMessage("At least one event subscription is required.");
    }
}

public class UpdateConfigurationRequestValidator : AbstractValidator<UpdateConfigurationRequest>
{
    public UpdateConfigurationRequestValidator()
    {
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100).WithMessage("Category is required.");
        RuleFor(x => x.Key).NotEmpty().MaximumLength(150).WithMessage("Configuration key is required.");
        RuleFor(x => x.Value).NotNull().WithMessage("Value cannot be null.");
    }
}

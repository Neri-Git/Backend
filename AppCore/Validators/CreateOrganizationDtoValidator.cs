using AppCore.Dto;
using FluentValidation;

namespace AppCore.Validators;

public class CreateOrganizationDtoValidator : AbstractValidator<CreateOrganizationDto>
{
    public CreateOrganizationDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Organization name is required.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email has invalid format.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Organization type is invalid.");
    }
}
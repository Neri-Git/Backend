using AppCore.Dto;
using FluentValidation;

namespace AppCore.Validators;

public class CreateCompanyDtoValidator : AbstractValidator<CreateCompanyDto>
{
    public CreateCompanyDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Company name is required.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email has invalid format.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required.");

        RuleFor(x => x.Nip)
            .NotEmpty()
            .WithMessage("NIP is required.");

        RuleFor(x => x.Regon)
            .MaximumLength(20)
            .WithMessage("REGON cannot be longer than 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Regon));
    }
}
using AppCore.Dto;
using AppCore.Interfaces;
using AppCore.Validators.Shared;
using FluentValidation;

namespace AppCore.Validators;

public class UpdatePersonDtoValidator : AbstractValidator<UpdatePersonDto>
{
    private readonly ICompanyRepository _companyRepository;

    public UpdatePersonDtoValidator(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Imię nie może przekraczać 100 znaków.")
            .Matches(@"^[\p{L}\s\-]+$").WithMessage("Imię zawiera niedozwolone znaki.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(200).WithMessage("Nazwisko nie może przekraczać 200 znaków.")
            .Matches(@"^[\p{L}\s\-]+$").WithMessage("Nazwisko zawiera niedozwolone znaki.")
            .When(x => x.LastName is not null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Nieprawidłowy format adresu email.")
            .MaximumLength(200).WithMessage("Email nie może przekraczać 200 znaków.")
            .When(x => x.Email is not null);

        RuleFor(x => x.Phone)
            .Matches(@"^(\+?\d{1,3}[\s-]?)?(\d{3}[\s-]?){2}\d{3}$")
            .WithMessage("Nieprawidłowy format numeru telefonu.")
            .When(x => x.Phone is not null);

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.Today.AddYears(-18))
                .WithMessage("Osoba musi mieć co najmniej 18 lat.")
            .GreaterThan(DateTime.Today.AddYears(-120))
                .WithMessage("Nieprawidłowa data urodzenia.")
            .When(x => x.BirthDate.HasValue);

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Nieprawidłowa wartość płci.")
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.EmployerId)
            .MustAsync(EmployerExistsAsync)
            .WithMessage("Wskazana firma nie istnieje.")
            .When(x => x.EmployerId.HasValue);

        RuleFor(x => x.Address)
            .SetValidator(new AddressDtoValidator())
            .When(x => x.Address is not null);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Nieprawidłowy status kontaktu.")
            .When(x => x.Status.HasValue);
    }

    private async Task<bool> EmployerExistsAsync(Guid? employerId, CancellationToken ct)
        => await _companyRepository.FindByIdAsync(employerId ?? Guid.Empty) is not null;
}
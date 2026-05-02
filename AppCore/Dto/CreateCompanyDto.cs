using AppCore.Models;
using AppCore.ValueObjects;

namespace AppCore.Dto;

public record CreateCompanyDto(
    string Name,
    string Email,
    string Phone,
    string Nip,
    string? Regon,
    AddressDto? Address
)
{
    public Company ToEntity()
    {
        return new Company
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Email = Email,
            Phone = Phone,
            Nip = Nip,
            Regon = Regon,
            Address = Address?.ToValueObject(),
            Status = ContactStatus.Active
        };
    }
}
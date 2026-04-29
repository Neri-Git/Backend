using AppCore.Models;
using AppCore.ValueObjects;

namespace AppCore.Dto;

public record CreateOrganizationDto(
    string Name,
    string Email,
    string Phone,
    OrganizationType Type,
    AddressDto? Address
)
{
    public Organization ToEntity()
    {
        return new Organization
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Email = Email,
            Phone = Phone,
            Type = Type,
            Address = Address?.ToValueObject(),
            Status = ContactStatus.Active
        };
    }
}
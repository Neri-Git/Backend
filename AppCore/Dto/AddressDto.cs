using AppCore.Models;
using AppCore.ValueObjects;

namespace AppCore.Dto;

public record AddressDto(
    string Street,
    string City,
    string PostalCode,
    string Country,
    AddressType Type
)
{
    public Address ToValueObject()
    {
        return new Address
        {
            Street = Street,
            City = City,
            PostalCode = PostalCode,
            Country = Country,
            Type = Type
        };
    }
}
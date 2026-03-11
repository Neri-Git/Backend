using AppCore.Models;
using AppCore.ValueObjects;

namespace AppCore.Dto;

public record CreatePersonDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Position,
    DateTime? BirthDate,
    Gender Gender,
    Guid? EmployerId,
    AddressDto? Address
)
{
    public Person ToEntity()
    {
        return new Person
        {
            Id = Guid.NewGuid(),
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            Phone = Phone,
            Position = Position,
            BirthDate = BirthDate,
            Gender = Gender,
            EmployerId = EmployerId
        };
    }
}
using AppCore.Models;
using AppCore.ValueObjects;

namespace AppCore.Dto;

public record UpdatePersonDto(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Position,
    DateTime? BirthDate,
    Gender? Gender,
    Guid? EmployerId,
    AddressDto? Address,
    ContactStatus? Status
)
{
    public void UpdateEntity(Person person)
    {
        if (FirstName != null) person.FirstName = FirstName;
        if (LastName != null) person.LastName = LastName;
        if (Email != null) person.Email = Email;
        if (Phone != null) person.Phone = Phone;
        if (Position != null) person.Position = Position;
        if (BirthDate != null) person.BirthDate = BirthDate;
        if (Gender != null) person.Gender = Gender.Value;
        if (EmployerId != null) person.EmployerId = EmployerId;
        if (Status != null) person.Status = Status.Value;

        if (Address != null)
        {
            person.Address = new Address
            {
                Street = Address.Street,
                City = Address.City,
                PostalCode = Address.PostalCode,
                Country = Address.Country,
                Type = Address.Type
            };
        }
    }
}
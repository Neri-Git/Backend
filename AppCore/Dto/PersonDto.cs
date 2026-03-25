using AppCore.Models;
using AppCore.ValueObjects;

namespace AppCore.Dto;

public record PersonDto : ContactBaseDto
{
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string? Position { get; init; }
    public DateTime? BirthDate { get; init; }
    public Gender Gender { get; init; }
    public Guid? EmployerId { get; init; }
    public List<NoteDto> Notes { get; init; } = new();

    public static PersonDto FromEntity(Person person)
    {
        return new PersonDto
        {
            Id = person.Id,
            Email = person.Email,
            Phone = person.Phone,
            Status = person.Status,
            CreatedAt = person.CreatedAt,
            FirstName = person.FirstName,
            LastName = person.LastName,
            Position = person.Position,
            BirthDate = person.BirthDate,
            Gender = person.Gender,
            EmployerId = person.EmployerId,
            Tags = person.Tags,
            Notes = person.Notes.Select(NoteDto.FromEntity).ToList()
        };
    }
}
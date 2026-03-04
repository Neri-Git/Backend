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
}
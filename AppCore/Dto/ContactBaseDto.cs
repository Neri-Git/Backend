using AppCore.ValueObjects;

namespace AppCore.Dto;

public abstract record ContactBaseDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public AddressDto? Address { get; init; }
    public ContactStatus Status { get; init; }
    public List<string> Tags { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}
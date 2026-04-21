using AppCore.Interfaces;

namespace AppCore.Dto;

using AppCore.Models;

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public SystemUserStatus Status { get; init; }
    public string Department { get; init; } = string.Empty;
    public IList<string> Roles { get; init; } = [];
}
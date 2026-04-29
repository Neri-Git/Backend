using AppCore.Common;
using AppCore.ValueObjects;

namespace AppCore.Models;

public abstract class Contact : EntityBase
{
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public Address? Address { get; set; }
    public ContactStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? CreatedByUserId { get; set; }

    public List<string> Tags { get; set; } = new();

    public List<Note> Notes { get; set; } = new();
}
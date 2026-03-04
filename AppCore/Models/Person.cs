using AppCore.ValueObjects;

namespace AppCore.Models;

public class Person : Contact
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Position { get; set; }
    public DateTime? BirthDate { get; set; }
    public Gender Gender { get; set; }

    public Guid? EmployerId { get; set; }
    public Guid? OrganizationId { get; set; }
}
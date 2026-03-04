using AppCore.ValueObjects;

namespace AppCore.Models;

public class Organization : Contact
{
    public string Name { get; set; } = default!;
    public OrganizationType Type { get; set; }
}
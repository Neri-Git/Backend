namespace AppCore.Models;

public class Company : Contact
{
    public string Name { get; set; } = default!;
    public string Nip { get; set; } = default!;
}
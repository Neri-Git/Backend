using AppCore.ValueObjects;

namespace AppCore.Models;

public class Address
{
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Country { get; set; } = default!;
    public AddressType Type { get; set; }
}
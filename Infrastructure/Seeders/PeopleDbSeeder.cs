namespace Infrastructure.Seeders;

using AppCore.Interfaces;
using AppCore.Models;
using AppCore.ValueObjects;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class PeopleDbSeeder : IDataSeeder
{
    public int Order => 2;

    private readonly ContactsDbContext _context;
    private readonly ILogger<PeopleDbSeeder> _logger;

    public PeopleDbSeeder(
        ContactsDbContext context,
        ILogger<PeopleDbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.People.AnyAsync())
        {
            _logger.LogInformation("Kontakty Person już istnieją — pomijam seedowanie.");
            return;
        }

        var people = new[]
        {
            new Person
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "Adam",
                LastName = "Nowak",
                Email = "adam.nowak@crm.pl",
                Phone = "123456789",
                Status = ContactStatus.Active,
                CreatedAt = DateTime.UtcNow,
                Gender = Gender.Male,
                Position = "Programista"
            },
            new Person
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Ewa",
                LastName = "Kowalska",
                Email = "ewa.kowalska@crm.pl",
                Phone = "987654321",
                Status = ContactStatus.Active,
                CreatedAt = DateTime.UtcNow,
                Gender = Gender.Female,
                Position = "Tester"
            },
            new Person
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                FirstName = "Piotr",
                LastName = "Wiśniewski",
                Email = "piotr.wisniewski@crm.pl",
                Phone = "555666777",
                Status = ContactStatus.Blocked,
                CreatedAt = DateTime.UtcNow,
                Gender = Gender.Male,
                Position = "Analityk"
            }
        };

        await _context.People.AddRangeAsync(people);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Dodano {Count} kontaktów typu Person.", people.Length);
    }
}
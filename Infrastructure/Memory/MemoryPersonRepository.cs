using AppCore.Interfaces;
using AppCore.Models;
using AppCore.ValueObjects;

namespace Infrastructure.Memory;

public class MemoryPersonRepository 
    : MemoryGenericRepository<Person>, IPersonRepository
{
    public MemoryPersonRepository()
    {
        var p1 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Adam",
            LastName = "Nowak",
            Gender = Gender.Male,
            Email = "adam.nowak@email.com",
            Phone = "111111111"
        };

        var p2 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Anna",
            LastName = "Kowalska",
            Gender = Gender.Female,
            Email = "anna.kowalska@email.com",
            Phone = "222222222"
        };

        _data.Add(p1.Id, p1);
        _data.Add(p2.Id, p2);
    }

    public async Task<IEnumerable<Person>> FindByCompanyAsync(Guid companyId)
    {
        var all = await FindAllAsync();
        return all.Where(p => p.EmployerId == companyId);
    }

    public async Task<IEnumerable<Person>> FindByOrganizationAsync(Guid organizationId)
    {
        var all = await FindAllAsync();
        return all.Where(p => p.OrganizationId == organizationId);
    }
}
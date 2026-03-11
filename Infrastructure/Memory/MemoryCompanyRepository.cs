using AppCore.Interfaces;
using AppCore.Models;

namespace Infrastructure.Memory;

public class MemoryCompanyRepository 
    : MemoryGenericRepository<Company>, ICompanyRepository
{
    public MemoryCompanyRepository()
    {
        var c1 = new Company
        {
            Id = Guid.NewGuid(),
            Name = "TechSoft",
            Nip = "1234567890",
            Email = "office@techsoft.com",
            Phone = "123456789"
        };

        var c2 = new Company
        {
            Id = Guid.NewGuid(),
            Name = "DataCorp",
            Nip = "9876543210",
            Email = "contact@datacorp.com",
            Phone = "987654321"
        };

        _data.Add(c1.Id, c1);
        _data.Add(c2.Id, c2);
    }

    public Task<IEnumerable<Company>> FindByNameAsync(string namePart)
    {
        var result = _data.Values
            .Where(c => c.Name.Contains(namePart, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(result);
    }

    public Task<Company?> FindByNipAsync(string nip)
    {
        var result = _data.Values
            .FirstOrDefault(c => c.Nip == nip);

        return Task.FromResult(result);
    }

    public Task<IEnumerable<Person>> GetEmployeesAsync(Guid companyId)
    {
        return Task.FromResult<IEnumerable<Person>>(new List<Person>());
    }
}
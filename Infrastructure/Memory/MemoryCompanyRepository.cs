using AppCore.Interfaces;
using AppCore.Models;

namespace Infrastructure.Memory;

public class MemoryCompanyRepository 
    : MemoryGenericRepository<Company>, ICompanyRepository
{
    public async Task<IEnumerable<Company>> FindByNameAsync(string namePart)
    {
        var all = await FindAllAsync();
        return all.Where(c => c.Name.Contains(namePart, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Company?> FindByNipAsync(string nip)
    {
        var all = await FindAllAsync();
        return all.FirstOrDefault(c => c.Nip == nip);
    }

    public async Task<IEnumerable<Person>> GetEmployeesAsync(Guid companyId)
    {
        throw new NotImplementedException("Use IPersonRepository to fetch employees.");
    }
}
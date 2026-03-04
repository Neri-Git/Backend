using AppCore.Interfaces;
using AppCore.Models;

namespace Infrastructure.Memory;

public class MemoryPersonRepository 
    : MemoryGenericRepository<Person>, IPersonRepository
{
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
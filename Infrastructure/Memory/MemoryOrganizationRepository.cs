using AppCore.Interfaces;
using AppCore.Models;
using AppCore.ValueObjects;

namespace Infrastructure.Memory;

public class MemoryOrganizationRepository 
    : MemoryGenericRepository<Organization>, IOrganizationRepository
{
    public async Task<IEnumerable<Organization>> FindByTypeAsync(OrganizationType type)
    {
        var all = await FindAllAsync();
        return all.Where(o => o.Type == type);
    }

    public async Task<IEnumerable<Person>> GetMembersAsync(Guid organizationId)
    {
        throw new NotImplementedException("Use IPersonRepository to fetch members.");
    }
}
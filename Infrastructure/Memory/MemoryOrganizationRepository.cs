using AppCore.Interfaces;
using AppCore.Models;
using AppCore.ValueObjects;

namespace Infrastructure.Memory;

public class MemoryOrganizationRepository 
    : MemoryGenericRepository<Organization>, IOrganizationRepository
{
    public MemoryOrganizationRepository()
    {
        var o1 = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Open Knowledge Foundation",
            Type = OrganizationType.Foundation,
            Email = "contact@okf.org",
            Phone = "111222333"
        };

        var o2 = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Developers Association",
            Type = OrganizationType.Association,
            Email = "info@devassoc.org",
            Phone = "444555666"
        };

        _data.Add(o1.Id, o1);
        _data.Add(o2.Id, o2);
    }

    public Task<IEnumerable<Organization>> FindByTypeAsync(OrganizationType type)
    {
        var result = _data.Values
            .Where(o => o.Type == type);

        return Task.FromResult(result);
    }

    public Task<IEnumerable<Person>> GetMembersAsync(Guid organizationId)
    {
        return Task.FromResult<IEnumerable<Person>>(new List<Person>());
    }
}
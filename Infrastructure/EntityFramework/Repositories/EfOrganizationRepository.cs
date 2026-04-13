using AppCore.Interfaces;
using AppCore.Models;
using AppCore.ValueObjects;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfOrganizationRepository(ContactsDbContext context)
    : EfGenericRepository<Organization>(context.Organizations), IOrganizationRepository
{
    public Task<IEnumerable<Organization>> FindByTypeAsync(OrganizationType type)
    {
        return Task.FromResult<IEnumerable<Organization>>(
            context.Organizations.AsNoTracking().Where(o => o.Type == type).ToList()
        );
    }

    public Task<IEnumerable<Person>> GetMembersAsync(Guid organizationId)
    {
        return Task.FromResult<IEnumerable<Person>>(
            context.People.AsNoTracking().Where(p => p.OrganizationId == organizationId).ToList()
        );
    }
}
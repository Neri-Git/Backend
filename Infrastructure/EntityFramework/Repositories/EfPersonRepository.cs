using AppCore.Interfaces;
using AppCore.Models;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfPersonRepository(ContactsDbContext context)
    : EfGenericRepository<Person>(context.People), IPersonRepository
{
    public Task<IEnumerable<Person>> FindByCompanyAsync(Guid companyId)
    {
        return Task.FromResult<IEnumerable<Person>>(
            context.People.AsNoTracking().Where(p => p.EmployerId == companyId).ToList()
        );
    }

    public Task<IEnumerable<Person>> FindByOrganizationAsync(Guid organizationId)
    {
        return Task.FromResult<IEnumerable<Person>>(
            context.People.AsNoTracking().Where(p => p.OrganizationId == organizationId).ToList()
        );
    }
}
using AppCore.Interfaces;
using AppCore.Models;
using Microsoft.EntityFrameworkCore;
using Infrastructure.EntityFramework.Context;

namespace Infrastructure.EntityFramework.Repositories;

public class EfCompanyRepository(ContactsDbContext context)
    : EfGenericRepository<Company>(context.Companies), ICompanyRepository
{
    public Task<Company?> FindByNipAsync(string nip)
    {
        return context.Companies
            .FirstOrDefaultAsync(c => c.Nip == nip);
    }

    public Task<IEnumerable<Company>> FindByNameAsync(string namePart)
    {
        return Task.FromResult<IEnumerable<Company>>(
            context.Companies
                .Where(c => c.Name.Contains(namePart))
                .ToList()
        );
    }

    public Task<IEnumerable<Person>> GetEmployeesAsync(Guid companyId)
    {
        return Task.FromResult<IEnumerable<Person>>(
            context.People
                .Where(p => p.EmployerId == companyId)
                .ToList()
        );
    }
}
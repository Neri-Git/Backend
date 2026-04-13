using AppCore.Models;
using Infrastructure.EntityFramework.Context;

namespace Infrastructure.EntityFramework.Repositories;

public class EfContactRepository(ContactsDbContext context)
    : EfGenericRepository<Contact>(context.Set<Contact>())
{
}
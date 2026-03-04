using AppCore.Interfaces;
using AppCore.Models;

public interface IPersonRepository 
    : IGenericRepositoryAsync<Person>
{
    Task<IEnumerable<Person>> FindByCompanyAsync(Guid companyId);
    Task<IEnumerable<Person>> FindByOrganizationAsync(Guid organizationId);
}
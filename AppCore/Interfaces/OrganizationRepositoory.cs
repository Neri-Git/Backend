using AppCore.Interfaces;
using AppCore.Models;
using AppCore.ValueObjects;

public interface IOrganizationRepository 
    : IGenericRepositoryAsync<Organization>
{
    Task<IEnumerable<Organization>> FindByTypeAsync(OrganizationType type);
    Task<IEnumerable<Person>> GetMembersAsync(Guid organizationId);
}
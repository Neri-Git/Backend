using AppCore.Interfaces;
using AppCore.Models;

public interface ICompanyRepository 
    : IGenericRepositoryAsync<Company>
{
    Task<IEnumerable<Company>> FindByNameAsync(string namePart);
    Task<Company?> FindByNipAsync(string nip);
    Task<IEnumerable<Person>> GetEmployeesAsync(Guid companyId);
}
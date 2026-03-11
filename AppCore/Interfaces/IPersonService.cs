using AppCore.Dto;

namespace AppCore.Interfaces;

public interface IPersonService
{
    Task<PagedResult<PersonDto>> FindAllPeoplePaged(int page, int size);
    Task<IAsyncEnumerable<PersonDto>> FindPeopleFromCompany(Guid companyId);
}
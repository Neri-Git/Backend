using AppCore.Dto;
using AppCore.Models;

public interface IPersonService
{
    Task<PagedResult<PersonDto>> FindAllPeoplePaged(int page, int size);
    Task<IAsyncEnumerable<PersonDto>> FindPeopleFromCompany(Guid companyId);

    Task<Person> AddPerson(CreatePersonDto personDto);
    Task<Person> UpdatePerson(UpdatePersonDto personDto);
    Task<PersonDto?> GetById(Guid id);
    
    Task<Note> AddNoteToPerson(Guid personId, CreateNoteDto noteDto);
    Task<PersonDto> GetPerson(Guid personId);
    Task DeleteNote(Guid personId, Guid noteId);
}
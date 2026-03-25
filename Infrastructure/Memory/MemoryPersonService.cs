using AppCore.Dto;
using AppCore.Exceptions;
using AppCore.Interfaces;
using AppCore.Models;

namespace Infrastructure.Memory;

public class MemoryPersonService(IContactUnitOfWork unitOfWork) : IPersonService
{
    public async Task<PagedResult<PersonDto>> FindAllPeoplePaged(int page, int size)
    {
        var people = await unitOfWork.Persons.FindPagedAsync(page, size);

        var items = people.Items
            .Select(PersonDto.FromEntity)
            .ToList();

        return new PagedResult<PersonDto>(
            items,
            people.TotalCount,
            people.Page,
            people.PageSize
        );
    }

    public async Task<IAsyncEnumerable<PersonDto>> FindPeopleFromCompany(Guid companyId)
    {
        var people = await unitOfWork.Persons.FindByCompanyAsync(companyId);
        return GetAsync(people);
    }

    public async Task<Person> AddPerson(CreatePersonDto personDto)
    {
        var entity = personDto.ToEntity();

        entity = await unitOfWork.Persons.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();

        return entity;
    }

    public async Task<Person> UpdatePerson(UpdatePersonDto personDto)
    {
        var existing = await unitOfWork.Persons.FindByIdAsync(personDto.Id);

        if (existing == null)
            throw new KeyNotFoundException("Person not found");

        personDto.UpdateEntity(existing);

        var updated = await unitOfWork.Persons.UpdateAsync(existing);
        await unitOfWork.SaveChangesAsync();

        return updated;
    }

    public async Task<PersonDto?> GetById(Guid id)
    {
        var person = await unitOfWork.Persons.FindByIdAsync(id);

        if (person == null)
            return null;

        return PersonDto.FromEntity(person);
    }

    public async Task<Note> AddNoteToPerson(Guid personId, CreateNoteDto noteDto)
    {
        var person = await unitOfWork.Persons.FindByIdAsync(personId);

        if (person is null)
            throw new ContactNotFoundException($"Person with id={personId} not found!");

        if (person.Notes is null)
            person.Notes = new List<Note>();

        var note = new Note
        {
            Id = Guid.NewGuid(),
            Content = noteDto.Content,
            CreatedAt = DateTime.UtcNow
        };

        person.Notes.Add(note);

        await unitOfWork.Persons.UpdateAsync(person);
        await unitOfWork.SaveChangesAsync();

        return note;
    }

    public async Task<PersonDto> GetPerson(Guid personId)
    {
        var person = await unitOfWork.Persons.FindByIdAsync(personId);

        if (person is null)
            throw new ContactNotFoundException($"Person with id={personId} not found!");

        return PersonDto.FromEntity(person);
    }

    private async IAsyncEnumerable<PersonDto> GetAsync(IEnumerable<Person> people)
    {
        foreach (var person in people)
        {
            yield return PersonDto.FromEntity(person);
            await Task.Yield();
        }
    }

    public async Task DeleteNote(Guid personId, Guid noteId)
    {
        var person = await unitOfWork.Persons.FindByIdAsync(personId);

        if (person is null)
            throw new ContactNotFoundException($"Person with id={personId} not found!");

        if (person.Notes is null || !person.Notes.Any())
            throw new Exception("No notes to delete.");

        var note = person.Notes.FirstOrDefault(n => n.Id == noteId);

        if (note is null)
            throw new Exception($"Note with id={noteId} not found.");

        person.Notes.Remove(note);

        await unitOfWork.Persons.UpdateAsync(person);
        await unitOfWork.SaveChangesAsync();
    }
}
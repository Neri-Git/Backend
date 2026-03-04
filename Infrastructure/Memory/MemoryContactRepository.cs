using AppCore.Dto;
using AppCore.Interfaces;
using AppCore.Models;

namespace Infrastructure.Memory;

public class MemoryContactRepository 
    : MemoryGenericRepository<Contact>, IContactRepository
{
    public async Task<PagedResult<Contact>> SearchAsync(ContactSearchDto search)
    {
        var all = await FindAllAsync();

        var query = all.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.Query))
            query = query.Where(c =>
                c.Email.Contains(search.Query, StringComparison.OrdinalIgnoreCase) ||
                c.Phone.Contains(search.Query, StringComparison.OrdinalIgnoreCase));

        if (search.Status.HasValue)
            query = query.Where(c => c.Status == search.Status.Value);

        if (!string.IsNullOrWhiteSpace(search.Tag))
            query = query.Where(c => c.Tags.Contains(search.Tag));

        var total = query.Count();

        var items = query
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .ToList();

        return new PagedResult<Contact>(items, total, search.Page, search.PageSize);
    }

    public async Task<IEnumerable<Contact>> FindByTagAsync(string tag)
    {
        var all = await FindAllAsync();
        return all.Where(c => c.Tags.Contains(tag));
    }

    public async Task AddNoteAsync(Guid contactId, Note note)
    {
        var contact = await FindByIdAsync(contactId)
            ?? throw new KeyNotFoundException("Contact not found");

        contact.Notes.Add(note);
        await UpdateAsync(contact);
    }

    public async Task<IEnumerable<Note>> GetNotesAsync(Guid contactId)
    {
        var contact = await FindByIdAsync(contactId)
            ?? throw new KeyNotFoundException("Contact not found");

        return contact.Notes;
    }

    public async Task AddTagAsync(Guid contactId, string tag)
    {
        var contact = await FindByIdAsync(contactId)
            ?? throw new KeyNotFoundException("Contact not found");

        if (!contact.Tags.Contains(tag))
            contact.Tags.Add(tag);

        await UpdateAsync(contact);
    }

    public async Task RemoveTagAsync(Guid contactId, string tag)
    {
        var contact = await FindByIdAsync(contactId)
            ?? throw new KeyNotFoundException("Contact not found");

        contact.Tags.Remove(tag);
        await UpdateAsync(contact);
    }
}
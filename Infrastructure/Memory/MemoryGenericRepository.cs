using AppCore.Dto;
using AppCore.Interfaces;

namespace Infrastructure.Memory;

public class MemoryGenericRepository<T> 
    : IGenericRepositoryAsync<T>
    where T : class
{
    private readonly Dictionary<Guid, T> _data = new();

    public Task<T?> FindByIdAsync(Guid id)
    {
        _data.TryGetValue(id, out var value);
        return Task.FromResult(value);
    }

    public Task<IEnumerable<T>> FindAllAsync()
    {
        return Task.FromResult(_data.Values.AsEnumerable());
    }

    public Task<PagedResult<T>> FindPagedAsync(int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var total = _data.Count;
        var items = _data.Values
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PagedResult<T>(items, total, page, pageSize);
        return Task.FromResult(result);
    }

    public Task<T> AddAsync(T entity)
    {
        var id = GetEntityId(entity);

        if (_data.ContainsKey(id))
            throw new InvalidOperationException("Entity already exists.");

        _data[id] = entity;
        return Task.FromResult(entity);
    }

    public Task<T> UpdateAsync(T entity)
    {
        var id = GetEntityId(entity);

        if (!_data.ContainsKey(id))
            throw new KeyNotFoundException("Entity not found.");

        _data[id] = entity;
        return Task.FromResult(entity);
    }

    public Task RemoveByIdAsync(Guid id)
    {
        if (!_data.Remove(id))
            throw new KeyNotFoundException("Entity not found.");

        return Task.CompletedTask;
    }

    private static Guid GetEntityId(T entity)
    {
        var prop = typeof(T).GetProperty("Id");
        if (prop == null)
            throw new InvalidOperationException("Entity must have Id property.");

        return (Guid)(prop.GetValue(entity)
                      ?? throw new InvalidOperationException("Id cannot be null."));
    }
}
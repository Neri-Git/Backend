using AppCore.Interfaces;
using AppCore.Models;
using Infrastructure.Memory;
using Xunit;

namespace AppCore.Tests.Repositories;

public class MemoryGenericRepositoryTests
{
    private readonly IGenericRepositoryAsync<Person> _repo;

    public MemoryGenericRepositoryTests()
    {
        _repo = new MemoryGenericRepository<Person>();
    }

    [Fact]
    public async Task AddPersonTestAsync()
    {
        var expected = new Person
        {
            FirstName = "Adam",
            LastName = "Nowak",
            Email = "adam@test.pl",
            Phone = "123456789"
        };

        await _repo.AddAsync(expected);

        var actual = await _repo.FindByIdAsync(expected.Id);

        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual!.Id);
        Assert.Equal("Adam", actual.FirstName);
    }

    [Fact]
    public async Task FindAllTestAsync()
    {
        var person1 = new Person { FirstName = "Jan", LastName = "Kowalski", Email = "jan@test.pl", Phone = "111" };
        var person2 = new Person { FirstName = "Anna", LastName = "Nowak", Email = "anna@test.pl", Phone = "222" };

        await _repo.AddAsync(person1);
        await _repo.AddAsync(person2);

        var result = await _repo.FindAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task RemoveByIdTestAsync()
    {
        var person = new Person { FirstName = "Test", LastName = "User", Email = "test@test.pl", Phone = "999" };

        await _repo.AddAsync(person);
        await _repo.RemoveByIdAsync(person.Id);

        var result = await _repo.FindByIdAsync(person.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTestAsync()
    {
        var person = new Person { FirstName = "Old", LastName = "Name", Email = "old@test.pl", Phone = "000" };
        await _repo.AddAsync(person);

        person.FirstName = "New";
        await _repo.UpdateAsync(person);

        var updated = await _repo.FindByIdAsync(person.Id);
        Assert.Equal("New", updated!.FirstName);
    }

    [Fact]
    public async Task FindPagedTestAsync()
    {
        for (int i = 0; i < 10; i++)
        {
            await _repo.AddAsync(new Person
            {
                FirstName = $"User{i}",
                LastName = "Test",
                Email = $"u{i}@test.pl",
                Phone = "123"
            });
        }

        var page = await _repo.FindPagedAsync(2, 3);

        Assert.Equal(3, page.Items.Count);
        Assert.Equal(10, page.TotalCount);
        Assert.Equal(2, page.Page);
    }
}
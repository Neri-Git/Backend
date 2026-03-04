using Infrastructure.Memory;
using AppCore.Interfaces;
using AppCore.Models;
using Xunit;

namespace AppCore.Tests;

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
        var person = new Person
        {
            FirstName = "Adam",
            LastName = "Nowak",
            Email = "adam@test.pl",
            Phone = "123456"
        };

        await _repo.AddAsync(person);

        var result = await _repo.FindByIdAsync(person.Id);

        Assert.NotNull(result);
        Assert.Equal("Adam", result!.FirstName);
    }
}
using System.Net;
using AppCore.Dto;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Entities;
using AppCore.Models;
using AppCore.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using WebAPI;
using Xunit;

public class ContactsApiTests : IClassFixture<ContactsAppTestFactory<Program>>
{
    private readonly HttpClient _client;

    public ContactsApiTests(ContactsAppTestFactory<Program> factory)
    {
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ContactsDbContext>();

        context.Database.EnsureCreated();

        if (!context.People.Any())
        {
            context.People.Add(new Person
            {
                Id = Guid.Parse("3D54091D-ABC8-49EC-9590-93AD3ED5458F"),
                FirstName = "Adam",
                LastName = "Nowak",
                Email = "adam@test.com",
                Phone = "123456789",
                CreatedAt = DateTime.UtcNow,
                Status = ContactStatus.Active,
                Address = new Address
                {
                    Street = "Testowa 1",
                    City = "Krakow",
                    PostalCode = "30-001",
                    Country = "Poland",
                    Type = AddressType.Correspondence
                }
            });

            context.SaveChanges();
        }
    }

    [Fact]
    public async Task GetPerson_ShouldReturnOk()
    {
        var id = Guid.Parse("3D54091D-ABC8-49EC-9590-93AD3ED5458F");

        var response = await _client.GetAsync($"/api/contacts/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetAllPersons_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/contacts?page=1&size=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetPerson_ShouldReturnCorrectPerson()
    {
        var id = Guid.Parse("3D54091D-ABC8-49EC-9590-93AD3ED5458F");

        var result = await _client.GetFromJsonAsync<PersonDto>($"/api/contacts/{id}");

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Adam", result.FirstName);
    }

    [Fact]
    public async Task GetPerson_ShouldReturnNotFound_WhenPersonDoesNotExist()
    {
        var id = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/contacts/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnCreated()
    {
        var dto = new CreatePersonDto(
            "Jan",
            "Kowalski",
            "jan@test.com",
            "999999999",
            null,
            null,
            default,
            null,
            null,
            null
        );
        var response = await _client.PostAsJsonAsync("/api/contacts", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<PersonDto>();

        Assert.NotNull(created);
        Assert.Equal("Jan", created.FirstName);
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnOk()
    {
        var id = Guid.Parse("3D54091D-ABC8-49EC-9590-93AD3ED5458F");

        var dto = new UpdatePersonDto(
            id,
            "Updated",
            "Nowak",
            "updated@test.com",
            "111111111",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var response = await _client.PutAsJsonAsync($"/api/contacts/{id}", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<PersonDto>();

        Assert.Equal("Updated", updated.FirstName);
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenIdsMismatch()
    {
        var id = Guid.NewGuid();

        var dto = new UpdatePersonDto(
            id,
            "Updated",
            "Nowak",
            "updated@test.com",
            "111111111",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var response = await _client.PutAsJsonAsync($"/api/contacts/{Guid.NewGuid()}", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddNote_ShouldReturnCreated()
    {
        var contactId = Guid.Parse("3D54091D-ABC8-49EC-9590-93AD3ED5458F");

        var dto = new CreateNoteDto("Test note");

        var response = await _client.PostAsJsonAsync($"/api/contacts/{contactId}/notes", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
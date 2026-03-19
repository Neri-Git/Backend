using AppCore.Dto;
using AppCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/contacts")]
public class ContactsController(IPersonService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllPersons(int page = 1, int size = 10)
    {
        return Ok(await service.FindAllPeoplePaged(page, size));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPerson(Guid id)
    {
        var dto = await service.GetById(id);

        if (dto is null)
            return NotFound();

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePersonDto dto)
    {
        var result = await service.AddPerson(dto);

        return CreatedAtAction(nameof(GetPerson), new { id = result.Id }, result);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePerson(Guid id, [FromBody] UpdatePersonDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Id w URL musi zgadzać się z Id w DTO.");

        var existingPerson = await service.GetById(id);
        if (existingPerson is null)
            return NotFound();

        var updated = await service.UpdatePerson(dto);
        var resultDto = PersonDto.FromEntity(updated);

        return Ok(resultDto);
    }
}
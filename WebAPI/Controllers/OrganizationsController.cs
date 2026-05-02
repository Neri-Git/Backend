using AppCore.Dto;
using Infrastructure.EntityFramework.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly ContactsDbContext _context;

    public OrganizationsController(ContactsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetOrganizations(
        CancellationToken cancellationToken)
    {
        var organizations = await _context.Organizations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(organizations.Select(OrganizationDto.FromEntity));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganizationDto>> GetOrganization(
        Guid id,
        CancellationToken cancellationToken)
    {
        var organization = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (organization is null)
        {
            return NotFound();
        }

        return Ok(OrganizationDto.FromEntity(organization));
    }
}
using System.Security.Claims;
using AppCore.Dto;
using AppCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/contact-import")]
public class ContactImportController : ControllerBase
{
    private readonly IContactImportService _contactImportService;

    public ContactImportController(IContactImportService contactImportService)
    {
        _contactImportService = contactImportService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImportContactsResultDto>> ImportContacts(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("Cannot identify importing user.");
        }

        await using var stream = file.OpenReadStream();

        var result = await _contactImportService.ImportAsync(
            stream,
            file.FileName,
            userId,
            cancellationToken);

        return Ok(result);
    }
}
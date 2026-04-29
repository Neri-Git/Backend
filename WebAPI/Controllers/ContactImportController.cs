using System.Security.Claims;
using AppCore.Dto;
using AppCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
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
    [ProducesResponseType(typeof(ImportContactsResultDto), StatusCodes.Status200OK)]
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
            userId = "anonymous";
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
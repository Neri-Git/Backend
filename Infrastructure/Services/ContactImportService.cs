using System.Globalization;
using System.Text.Json;
using AppCore.Dto;
using AppCore.Interfaces;
using AppCore.Models;
using AppCore.ValueObjects;
using FluentValidation;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Infrastructure.Services;

public class ContactImportService : IContactImportService
{
    private readonly ContactsDbContext _context;
    private readonly IValidator<CreatePersonDto> _personValidator;
    private readonly IValidator<CreateCompanyDto> _companyValidator;
    private readonly IValidator<CreateOrganizationDto> _organizationValidator;

    public ContactImportService(
        ContactsDbContext context,
        IValidator<CreatePersonDto> personValidator,
        IValidator<CreateCompanyDto> companyValidator,
        IValidator<CreateOrganizationDto> organizationValidator)
    {
        _context = context;
        _personValidator = personValidator;
        _companyValidator = companyValidator;
        _organizationValidator = organizationValidator;
    }

    public async Task<ImportContactsResultDto> ImportAsync(
        Stream fileStream,
        string fileName,
        string importedByUserId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".json" => await ImportJsonAsync(fileStream, importedByUserId, cancellationToken),
            ".csv" => await ImportCsvAsync(fileStream, importedByUserId, cancellationToken),
            _ => new ImportContactsResultDto
            {
                Errors =
                {
                    new ImportContactErrorDto
                    {
                        ContactType = "File",
                        Messages = { "Unsupported file type. Only CSV and JSON files are allowed." }
                    }
                }
            }
        };
    }

    private async Task<ImportContactsResultDto> ImportJsonAsync(
        Stream fileStream,
        string importedByUserId,
        CancellationToken cancellationToken)
    {
        var result = new ImportContactsResultDto();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new JsonStringEnumConverter());

        ImportContactsJsonDto? importData;

        try
        {
            importData = await JsonSerializer.DeserializeAsync<ImportContactsJsonDto>(
                fileStream,
                options,
                cancellationToken);
        }
        catch (JsonException ex)
        {
            result.Errors.Add(new ImportContactErrorDto
            {
                ContactType = "Json",
                Messages = { $"Invalid JSON file: {ex.Message}" }
            });

            return result;
        }

        if (importData is null)
        {
            result.Errors.Add(new ImportContactErrorDto
            {
                ContactType = "Json",
                Messages = { "JSON file is empty or has invalid structure." }
            });

            return result;
        }

        var importedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var personDto in importData.People)
        {
            await ImportPersonAsync(personDto, result, importedKeys, importedByUserId, cancellationToken);
        }

        foreach (var companyDto in importData.Companies)
        {
            await ImportCompanyAsync(companyDto, result, importedKeys, importedByUserId, cancellationToken);
        }

        foreach (var organizationDto in importData.Organizations)
        {
            await ImportOrganizationAsync(organizationDto, result, importedKeys, importedByUserId, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }

    private async Task<ImportContactsResultDto> ImportCsvAsync(
        Stream fileStream,
        string importedByUserId,
        CancellationToken cancellationToken)
    {
        var result = new ImportContactsResultDto();
        var importedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(fileStream);

        string? currentGroup = null;
        List<string>? headers = null;
        char delimiter = ';';

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            line = line.Trim();

            if (IsGroupName(line))
            {
                currentGroup = NormalizeGroupName(line);
                headers = null;
                continue;
            }

            if (currentGroup is null)
            {
                result.Errors.Add(new ImportContactErrorDto
                {
                    ContactType = "Csv",
                    SourceData = new Dictionary<string, string?> { { "Line", line } },
                    Messages = { "Contact group name is missing. Expected People, Companies or Organizations." }
                });

                continue;
            }

            if (headers is null)
            {
                delimiter = DetectDelimiter(line);
                headers = SplitCsvLine(line, delimiter)
                    .Select(x => x.Trim())
                    .ToList();

                continue;
            }

            var values = SplitCsvLine(line, delimiter);

            if (values.Count != headers.Count)
            {
                result.Errors.Add(new ImportContactErrorDto
                {
                    ContactType = currentGroup,
                    SourceData = new Dictionary<string, string?> { { "Line", line } },
                    Messages = { "Number of values does not match number of headers." }
                });

                continue;
            }

            var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < headers.Count; i++)
            {
                row[headers[i]] = values[i].Trim();
            }

            switch (currentGroup)
            {
                case "People":
                    await ImportPersonAsync(MapPerson(row), result, importedKeys, importedByUserId, cancellationToken, row);
                    break;

                case "Companies":
                    await ImportCompanyAsync(MapCompany(row), result, importedKeys, importedByUserId, cancellationToken, row);
                    break;

                case "Organizations":
                    await ImportOrganizationAsync(MapOrganization(row), result, importedKeys, importedByUserId, cancellationToken, row);
                    break;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }

    private async Task ImportPersonAsync(
        CreatePersonDto dto,
        ImportContactsResultDto result,
        HashSet<string> importedKeys,
        string importedByUserId,
        CancellationToken cancellationToken,
        Dictionary<string, string?>? sourceData = null)
    {
        var validation = await _personValidator.ValidateAsync(dto, cancellationToken);

        var messages = validation.Errors
            .Select(x => x.ErrorMessage)
            .ToList();

        var duplicateKey = $"Person:Email:{dto.Email}";

        if (!importedKeys.Add(duplicateKey))
        {
            messages.Add("Duplicated person in imported file.");
        }

        var existsInDatabase = await _context.People
            .AnyAsync(x => x.Email == dto.Email, cancellationToken);

        if (existsInDatabase)
        {
            messages.Add("Person with this email already exists in database.");
        }

        if (messages.Any())
        {
            result.Errors.Add(new ImportContactErrorDto
            {
                ContactType = "Person",
                SourceData = sourceData ?? ToDictionary(dto),
                Messages = messages
            });

            return;
        }

        var person = dto.ToEntity();
        person.CreatedByUserId = importedByUserId;

        _context.People.Add(person);

        result.ImportedContacts.Add(new ImportedContactSummaryDto
        {
            Id = person.Id,
            Name = $"{person.FirstName} {person.LastName}",
            Type = "Person"
        });
    }

    private async Task ImportCompanyAsync(
        CreateCompanyDto dto,
        ImportContactsResultDto result,
        HashSet<string> importedKeys,
        string importedByUserId,
        CancellationToken cancellationToken,
        Dictionary<string, string?>? sourceData = null)
    {
        var validation = await _companyValidator.ValidateAsync(dto, cancellationToken);

        var messages = validation.Errors
            .Select(x => x.ErrorMessage)
            .ToList();

        var duplicateEmailKey = $"Company:Email:{dto.Email}";
        var duplicateNipKey = $"Company:Nip:{dto.Nip}";

        if (!importedKeys.Add(duplicateEmailKey) || !importedKeys.Add(duplicateNipKey))
        {
            messages.Add("Duplicated company in imported file.");
        }

        var existsInDatabase = await _context.Companies
            .AnyAsync(x => x.Email == dto.Email || x.Nip == dto.Nip, cancellationToken);

        if (existsInDatabase)
        {
            messages.Add("Company with this email or NIP already exists in database.");
        }

        if (messages.Any())
        {
            result.Errors.Add(new ImportContactErrorDto
            {
                ContactType = "Company",
                SourceData = sourceData ?? ToDictionary(dto),
                Messages = messages
            });

            return;
        }

        var company = dto.ToEntity();
        company.CreatedByUserId = importedByUserId;

        _context.Companies.Add(company);

        result.ImportedContacts.Add(new ImportedContactSummaryDto
        {
            Id = company.Id,
            Name = company.Name,
            Type = "Company"
        });
    }

    private async Task ImportOrganizationAsync(
        CreateOrganizationDto dto,
        ImportContactsResultDto result,
        HashSet<string> importedKeys,
        string importedByUserId,
        CancellationToken cancellationToken,
        Dictionary<string, string?>? sourceData = null)
    {
        var validation = await _organizationValidator.ValidateAsync(dto, cancellationToken);

        var messages = validation.Errors
            .Select(x => x.ErrorMessage)
            .ToList();

        var duplicateEmailKey = $"Organization:Email:{dto.Email}";
        var duplicateNameKey = $"Organization:Name:{dto.Name}";

        if (!importedKeys.Add(duplicateEmailKey) || !importedKeys.Add(duplicateNameKey))
        {
            messages.Add("Duplicated organization in imported file.");
        }

        var existsInDatabase = await _context.Organizations
            .AnyAsync(x => x.Email == dto.Email || x.Name == dto.Name, cancellationToken);

        if (existsInDatabase)
        {
            messages.Add("Organization with this email or name already exists in database.");
        }

        if (messages.Any())
        {
            result.Errors.Add(new ImportContactErrorDto
            {
                ContactType = "Organization",
                SourceData = sourceData ?? ToDictionary(dto),
                Messages = messages
            });

            return;
        }

        var organization = dto.ToEntity();
        organization.CreatedByUserId = importedByUserId;

        _context.Organizations.Add(organization);

        result.ImportedContacts.Add(new ImportedContactSummaryDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Type = "Organization"
        });
    }

    private static CreatePersonDto MapPerson(Dictionary<string, string?> row)
    {
        return new CreatePersonDto(
            FirstName: Get(row, "FirstName"),
            LastName: Get(row, "LastName"),
            Email: Get(row, "Email"),
            Phone: Get(row, "Phone"),
            Position: GetNullable(row, "Position"),
            BirthDate: GetNullableDate(row, "BirthDate"),
            Gender: GetEnumOrDefault<Gender>(row, "Gender"),
            EmployerId: GetNullableGuid(row, "EmployerId"),
            OrganizationId: GetNullableGuid(row, "OrganizationId"),
            Address: null
        );
    }

    private static CreateCompanyDto MapCompany(Dictionary<string, string?> row)
    {
        return new CreateCompanyDto(
            Name: Get(row, "Name"),
            Email: Get(row, "Email"),
            Phone: Get(row, "Phone"),
            Nip: Get(row, "Nip"),
            Address: null
        );
    }

    private static CreateOrganizationDto MapOrganization(Dictionary<string, string?> row)
    {
        return new CreateOrganizationDto(
            Name: Get(row, "Name"),
            Email: Get(row, "Email"),
            Phone: Get(row, "Phone"),
            Type: GetEnumOrDefault<OrganizationType>(row, "Type"),
            Address: null
        );
    }

    private static string Get(Dictionary<string, string?> row, string key)
    {
        return row.TryGetValue(key, out var value) ? value ?? string.Empty : string.Empty;
    }

    private static string? GetNullable(Dictionary<string, string?> row, string key)
    {
        return row.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
    }

    private static DateTime? GetNullableDate(Dictionary<string, string?> row, string key)
    {
        var value = GetNullable(row, key);

        if (value is null)
        {
            return null;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    private static Guid? GetNullableGuid(Dictionary<string, string?> row, string key)
    {
        var value = GetNullable(row, key);

        if (value is null)
        {
            return null;
        }

        return Guid.TryParse(value, out var guid)
            ? guid
            : null;
    }

    private static TEnum GetEnumOrDefault<TEnum>(Dictionary<string, string?> row, string key)
        where TEnum : struct, Enum
    {
        var value = GetNullable(row, key);

        if (value is null)
        {
            return default;
        }

        return Enum.TryParse<TEnum>(value, true, out var parsedValue)
            ? parsedValue
            : default;
    }

    private static bool IsGroupName(string line)
    {
        var normalized = NormalizeGroupName(line);

        return normalized is "People" or "Companies" or "Organizations";
    }

    private static string NormalizeGroupName(string line)
    {
        return line.Trim().ToLowerInvariant() switch
        {
            "people" => "People",
            "persons" => "People",
            "person" => "People",

            "companies" => "Companies",
            "company" => "Companies",

            "organizations" => "Organizations",
            "organisations" => "Organizations",
            "organization" => "Organizations",
            "organisation" => "Organizations",

            _ => line.Trim()
        };
    }

    private static char DetectDelimiter(string headerLine)
    {
        var possibleDelimiters = new[] { ';', '|', '\t', ':' };

        return possibleDelimiters
            .OrderByDescending(delimiter => headerLine.Count(x => x == delimiter))
            .FirstOrDefault();
    }

    private static List<string> SplitCsvLine(string line, char delimiter)
    {
        return line.Split(delimiter).ToList();
    }

    private static Dictionary<string, string?> ToDictionary<T>(T dto)
    {
        return typeof(T)
            .GetProperties()
            .ToDictionary(
                property => property.Name,
                property => property.GetValue(dto)?.ToString());
    }
}
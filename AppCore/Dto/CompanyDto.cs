using AppCore.Models;

namespace AppCore.Dto;

public record CompanyDto : ContactBaseDto
{
    public string Name { get; init; } = default!;
    public string Nip { get; init; } = default!;
    public string? Regon { get; init; }

    public List<NoteDto> Notes { get; init; } = new();

    public static CompanyDto FromEntity(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            Email = company.Email,
            Phone = company.Phone,
            Address = company.Address is null ? null : AddressDto.FromValueObject(company.Address),
            Status = company.Status,
            CreatedAt = company.CreatedAt,
            CreatedByUserId = company.CreatedByUserId,
            Tags = company.Tags,
            Name = company.Name,
            Nip = company.Nip,
            Regon = company.Regon,
            Notes = company.Notes.Select(NoteDto.FromEntity).ToList()
        };
    }
}
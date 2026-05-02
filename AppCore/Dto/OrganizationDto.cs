using AppCore.Models;
using AppCore.ValueObjects;

namespace AppCore.Dto;

public record OrganizationDto : ContactBaseDto
{
    public string Name { get; init; } = default!;
    public OrganizationType Type { get; init; }

    public List<NoteDto> Notes { get; init; } = new();

    public static OrganizationDto FromEntity(Organization organization)
    {
        return new OrganizationDto
        {
            Id = organization.Id,
            Email = organization.Email,
            Phone = organization.Phone,
            Address = organization.Address is null ? null : AddressDto.FromValueObject(organization.Address),
            Status = organization.Status,
            CreatedAt = organization.CreatedAt,
            CreatedByUserId = organization.CreatedByUserId,
            Tags = organization.Tags,
            Name = organization.Name,
            Type = organization.Type,
            Notes = organization.Notes.Select(NoteDto.FromEntity).ToList()
        };
    }
}
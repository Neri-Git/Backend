namespace AppCore.Dto;

public class ImportContactsJsonDto
{
    public List<CreatePersonDto> People { get; set; } = new();

    public List<CreateCompanyDto> Companies { get; set; } = new();

    public List<CreateOrganizationDto> Organizations { get; set; } = new();
}
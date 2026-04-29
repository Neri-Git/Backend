namespace AppCore.Dto;

public class ImportContactsResultDto
{
    public List<ImportedContactSummaryDto> ImportedContacts { get; set; } = new();

    public List<ImportContactErrorDto> Errors { get; set; } = new();

    public int ImportedCount => ImportedContacts.Count;

    public int ErrorCount => Errors.Count;
}
namespace AppCore.Dto;

public class ImportContactErrorDto
{
    public string ContactType { get; set; } = string.Empty;

    public Dictionary<string, string?> SourceData { get; set; } = new();

    public List<string> Messages { get; set; } = new();
}
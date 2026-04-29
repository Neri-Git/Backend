using AppCore.Dto;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppCore.Interfaces;

public interface IContactImportService
{
    Task<ImportContactsResultDto> ImportAsync(
        Stream fileStream,
        string fileName,
        string importedByUserId,
        CancellationToken cancellationToken = default);
}
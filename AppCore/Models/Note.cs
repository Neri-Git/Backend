using AppCore.Common;

namespace AppCore.Models;

public class Note : EntityBase
{
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
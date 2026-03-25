using AppCore.Models;

namespace AppCore.Dto;

public record NoteDto(
    Guid Id,
    string Content,
    DateTime CreatedAt
)
{
    public static NoteDto FromEntity(Note note)
        => new(note.Id, note.Content, note.CreatedAt);
}
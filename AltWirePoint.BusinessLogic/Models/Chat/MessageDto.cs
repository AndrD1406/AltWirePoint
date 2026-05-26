namespace AltWirePoint.BusinessLogic.Models.Chat;

public class MessageDto
{
    public Guid Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public Guid SenderId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string? SenderProfilePictureUrl { get; set; }

    public Guid ChatId { get; set; }

    public List<string>? FileUrls { get; set; }
}

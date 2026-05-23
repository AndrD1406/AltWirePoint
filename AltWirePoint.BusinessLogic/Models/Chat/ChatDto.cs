namespace AltWirePoint.BusinessLogic.Models.Chat;

public class ChatDto
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<ParticipantDto> Participants { get; set; } = new();

    public MessageDto? LastMessage { get; set; }

    public int UnreadCount { get; set; }
}

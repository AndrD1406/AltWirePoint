namespace AltWirePoint.BusinessLogic.Models.Chat;

public class CreateChatRequest
{
    public List<Guid> ParticipantIds { get; set; } = new();

    public string? Name { get; set; } // null for 1-1
}

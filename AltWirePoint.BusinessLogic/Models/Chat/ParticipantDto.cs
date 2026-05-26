namespace AltWirePoint.BusinessLogic.Models.Chat;

public class ParticipantDto
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? ProfilePictureUrl { get; set; }
}

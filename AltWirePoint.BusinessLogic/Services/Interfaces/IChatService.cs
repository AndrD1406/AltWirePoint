using AltWirePoint.BusinessLogic.Models;
using AltWirePoint.BusinessLogic.Models.Chat;

namespace AltWirePoint.BusinessLogic.Services.Interfaces;

public interface IChatService
{
    /// <summary>
    /// Creates a new Chat between the specified participants.
    /// </summary>
    Task<ChatDto> CreateChat(Guid creatorId, CreateChatRequest request);

    /// <summary>
    /// Gets all Chats for a user, ordered by most recent message.
    /// </summary>
    Task<IEnumerable<ChatDto>> GetChatsForUser(Guid userId, int skip = 0, int take = 20);

    /// <summary>
    /// Gets just the Chat IDs for a user (used by the Hub on connect).
    /// </summary>
    Task<IEnumerable<Guid>> GetChatIdsForUser(Guid userId);

    /// <summary>
    /// Gets the messages for a Chat with pagination.
    /// </summary>
    Task<IEnumerable<MessageDto>> GetMessages(Guid ChatId, Guid userId, int skip = 0, int take = 50);

    /// <summary>
    /// Sends a message to a Chat. Returns the DTO to broadcast via SignalR.
    /// </summary>
    Task<MessageDto> SendMessage(Guid ChatId, Guid senderId, string? content, IEnumerable<FileUploadDto>? files = null);

    /// <summary>
    /// Marks all messages in a Chat as read for the specified user.
    /// </summary>
    Task MarkChatAsRead(Guid ChatId, Guid userId);
}

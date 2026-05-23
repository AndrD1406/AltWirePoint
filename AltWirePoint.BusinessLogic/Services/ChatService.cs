using AltWirePoint.BusinessLogic.Models;
using AltWirePoint.BusinessLogic.Models.Chat;
using AltWirePoint.BusinessLogic.Services.Interfaces;
using AltWirePoint.DataAccess.Enums;
using AltWirePoint.DataAccess.Identity;
using AltWirePoint.DataAccess.Models;
using AltWirePoint.DataAccess.Repository.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AltWirePoint.BusinessLogic.Services;

public class ChatService : IChatService
{
    private readonly IEntityRepository<Guid, Chat> chatRepository;
    private readonly IEntityRepository<Guid, Message> messageRepository;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ICloudStoredFileService cloudStoredFileService;

    public ChatService(
        IEntityRepository<Guid, Chat> chatRepository,
        IEntityRepository<Guid, Message> messageRepository,
        UserManager<ApplicationUser> userManager,
        ICloudStoredFileService cloudStoredFileService)
    {
        this.chatRepository = chatRepository;
        this.messageRepository = messageRepository;
        this.userManager = userManager;
        this.cloudStoredFileService = cloudStoredFileService;
    }

    public async Task<ChatDto> CreateChat(Guid creatorId, CreateChatRequest request)
    {
        // Include the creator in the participant list
        var allParticipantIds = request.ParticipantIds
            .Append(creatorId)
            .Distinct()
            .ToList();

        var participants = await userManager.Users
            .Where(u => allParticipantIds.Contains(u.Id))
            .ToListAsync();

        if (participants.Count != allParticipantIds.Count)
            throw new ArgumentException("One or more participant IDs are invalid.");

        // For 1-on-1 Chats, check if one already exists between these two users
        if (allParticipantIds.Count == 2 && string.IsNullOrEmpty(request.Name))
        {
            var existing = await chatRepository
                .GetByFilter(
                    c => c.Participants.Count == 2
                      && c.Participants.Any(p => p.Id == allParticipantIds[0])
                      && c.Participants.Any(p => p.Id == allParticipantIds[1]),
                    includeExpression: q => q
                        .Include(c => c.Participants).ThenInclude(p => p.ProfilePicture))
                .FirstOrDefaultAsync();

            if (existing != null)
                return await MapToChatDto(existing, creatorId);
        }

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            Participants = participants.Cast<ApplicationUser>().ToList(),
        };

        await chatRepository.Create(chat);

        return await MapToChatDto(chat, creatorId);
    }

    public async Task<IEnumerable<ChatDto>> GetChatsForUser(Guid userId)
    {
        var chats = await chatRepository
            .GetByFilter(
                c => c.Participants.Any(p => p.Id == userId),
                includeExpression: q => q
                    .Include(c => c.Participants).ThenInclude(p => p.ProfilePicture)
                    .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                        .ThenInclude(m => m.Sender).ThenInclude(s => s.ProfilePicture))
            .AsNoTracking()
            .ToListAsync();

        var dtos = new List<ChatDto>();
        foreach (var chat in chats)
        {
            dtos.Add(await MapToChatDto(chat, userId));
        }

        // Order by last message time (most recent first), then by creation date
        return dtos.OrderByDescending(c => c.LastMessage?.SentAt ?? c.CreatedAt);
    }

    public async Task<IEnumerable<Guid>> GetChatIdsForUser(Guid userId)
    {
        return await chatRepository
            .GetByFilter(c => c.Participants.Any(p => p.Id == userId))
            .AsNoTracking()
            .Select(c => c.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<MessageDto>> GetMessages(Guid chatId, Guid userId, int skip = 0, int take = 50)
    {
        // Verify the user is a participant
        var isParticipant = await chatRepository
            .Any(c => c.Id == chatId && c.Participants.Any(p => p.Id == userId));

        if (!isParticipant)
            throw new UnauthorizedAccessException("You are not a participant in this Chat.");

        var messages = await messageRepository
            .Get(
                skip: skip,
                take: take,
                includeProperties: "Sender.ProfilePicture,Attachments",
                whereExpression: m => m.ChatId == chatId,
                orderBy: new List<(Expression<Func<Message, object>> Key, SortDirection Direction)>
                {
                    (m => m.SentAt, SortDirection.Descending)
                })
            .AsNoTracking()
            .ToListAsync();

        return messages
            .OrderBy(m => m.SentAt) // Return in chronological order for display
            .Select(MapToMessageDto);
    }

    public async Task<MessageDto> SendMessage(Guid chatId, Guid senderId, string? content, IEnumerable<FileUploadDto>? files = null)
    {
        if (files != null && files.Count() > 4)
            throw new ArgumentException("Maximum of 4 files allowed.");

        // Verify the sender is a participant
        var isParticipant = await chatRepository
            .Any(c => c.Id == chatId && c.Participants.Any(p => p.Id == senderId));

        if (!isParticipant)
            throw new UnauthorizedAccessException("You are not a participant in this Chat.");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            SenderId = senderId,
            Content = content ?? string.Empty,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            Attachments = new List<CloudStoredFile>()
        };

        if (files != null && files.Any())
        {
            var uploadTasks = files.Select(async file =>
            {
                var storedFile = await cloudStoredFileService.UploadFileAsync(file.Content, file.FileName, file.ContentType, CloudStoredFileService.ContainerNames.Messages);
                storedFile.MessageId = message.Id;
                return storedFile;
            });

            var storedFiles = await Task.WhenAll(uploadTasks);
            foreach (var storedFile in storedFiles)
            {
                message.Attachments.Add(storedFile);
            }
        }

        await messageRepository.Create(message);

        // Reload with sender info and attachments for the DTO
        var savedMessage = await messageRepository
            .GetByIdWithDetails(message.Id, includeProperties: "Sender.ProfilePicture,Attachments");

        return MapToMessageDto(savedMessage);
    }

    public async Task MarkChatAsRead(Guid chatId, Guid userId)
    {
        var unreadMessages = await messageRepository
            .GetByFilter(m => m.ChatId == chatId
                           && m.SenderId != userId
                           && !m.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        if (unreadMessages.Count > 0)
            await messageRepository.SaveChangesAsync();
    }

    private async Task<ChatDto> MapToChatDto(Chat chat, Guid currentUserId)
    {
        var lastMessage = chat.Messages?
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefault();

        var unreadCount = await messageRepository
            .Count(m => m.ChatId == chat.Id
                     && m.SenderId != currentUserId
                     && !m.IsRead);

        return new ChatDto
        {
            Id = chat.Id,
            Name = chat.Name,
            CreatedAt = chat.CreatedAt,
            Participants = chat.Participants?
                .Select(p => new ParticipantDto
                {
                    Id = p.Id,
                    UserName = p.UserName ?? string.Empty,
                    Name = p.Name,
                    ProfilePictureUrl = p.ProfilePicture?.Url
                })
                .ToList() ?? new(),
            LastMessage = lastMessage != null ? MapToMessageDto(lastMessage) : null,
            UnreadCount = unreadCount
        };
    }

    private static MessageDto MapToMessageDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead,
            SenderId = message.SenderId,
            SenderName = !string.IsNullOrWhiteSpace(message.Sender?.Name) ? message.Sender.Name : (message.Sender?.UserName ?? string.Empty),
            SenderProfilePictureUrl = message.Sender?.ProfilePicture?.Url,
            ChatId = message.ChatId,
            FileUrls = message.Attachments?.Select(a => a.Url).ToList()
        };
    }
}

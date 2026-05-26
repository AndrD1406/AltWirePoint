using AltWirePoint.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AltWirePoint.BusinessLogic.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService chatService;

    public ChatHub(IChatService chatService)
    {
        this.chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();

        // Add the user to SignalR groups for each of their Chats
        var chatIds = await chatService.GetChatIdsForUser(userId);
        foreach (var chatId in chatIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        await base.OnConnectedAsync();
    }

    public async Task Typing(Guid chatId)
    {
        var userId = GetUserId();
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Someone";

        await Clients.OthersInGroup(chatId.ToString())
            .SendAsync("UserTyping", new { UserId = userId, UserName = userName, chatId = chatId });
    }

    public async Task MarkAsRead(Guid chatId)
    {
        var userId = GetUserId();
        await chatService.MarkChatAsRead(chatId, userId);

        await Clients.OthersInGroup(chatId.ToString())
            .SendAsync("MessagesRead", new { UserId = userId, chatId = chatId });
    }

    public async Task JoinChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("User is not authenticated.");
        return Guid.Parse(userIdClaim);
    }
}

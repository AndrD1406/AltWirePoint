using AltWirePoint.BusinessLogic.Models.Chat;
using AltWirePoint.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using AltWirePoint.BusinessLogic.Hubs;
using AltWirePoint.BusinessLogic.Models;

namespace AltWirePoint.WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService chatService;
    private readonly IHubContext<ChatHub> hubContext;

    public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
    {
        this.chatService = chatService;
        this.hubContext = hubContext;
    }

    /// <summary>
    /// Creates a new Chat with the specified participants.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChatDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var Chat = await chatService.CreateChat(userId.Value, request);
        return Created(string.Empty, Chat);
    }

    /// <summary>
    /// Gets all Chats for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ChatDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChats()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var Chats = await chatService.GetChatsForUser(userId.Value);
        return Ok(Chats);
    }

    /// <summary>
    /// Gets messages for a specific Chat with pagination.
    /// </summary>
    [HttpGet("{ChatId}")]
    [ProducesResponseType(typeof(IEnumerable<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMessages(Guid ChatId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var messages = await chatService.GetMessages(ChatId, userId.Value, skip, take);
            return Ok(messages);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Marks all messages in a Chat as read for the current user.
    /// </summary>
    [HttpPut("{ChatId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAsRead(Guid ChatId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await chatService.MarkChatAsRead(ChatId, userId.Value);
        return NoContent();
    }

    /// <summary>
    /// Sends a message with optional file attachments.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendMessage([FromForm] Guid chatId, [FromForm] string? content, [FromForm] List<IFormFile>? files)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (files != null && files.Count > 4)
            return BadRequest("Maximum of 4 files allowed.");

        var fileDtos = new List<FileUploadDto>();
        if (files != null)
        {
            foreach (var file in files)
            {
                fileDtos.Add(new FileUploadDto
                {
                    Content = file.OpenReadStream(),
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length
                });
            }
        }

        try
        {
            var messageDto = await chatService.SendMessage(chatId, userId.Value, content, fileDtos);

            // Broadcast to the SignalR group
            await hubContext.Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", messageDto);

            return Created(string.Empty, messageDto);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return null;
        return userId;
    }
}

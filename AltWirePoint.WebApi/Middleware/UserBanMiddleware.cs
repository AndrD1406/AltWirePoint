using System.Security.Claims;
using AltWirePoint.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace AltWirePoint.WebApi.Middleware;

public class UserBanMiddleware
{
    private readonly RequestDelegate _next;

    public UserBanMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AltWirePointDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var isBanned = await dbContext.UserBans.AnyAsync(b =>
                    b.UserId == userId &&
                    b.IsActive &&
                    (b.BannedUntil == null || b.BannedUntil > DateTime.UtcNow));

                if (isBanned)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { error = "This account has been banned." });
                    return;
                }
            }
        }

        await _next(context);
    }
}

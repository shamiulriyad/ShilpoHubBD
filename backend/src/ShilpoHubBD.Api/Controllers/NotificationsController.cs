using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(ShilpoHubDbContext db) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool unreadOnly = false, [FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Clamp(page, 1, 10000);
        var asOf = DateTime.UtcNow;
        var owned = db.UserNotifications.AsNoTracking().Where(x => x.UserId == UserId && x.CreatedAt <= asOf);
        var unreadCount = await owned.CountAsync(x => x.ReadAt == null, cancellationToken);
        var filtered = unreadOnly ? owned.Where(x => x.ReadAt == null) : owned;
        var totalCount = await filtered.CountAsync(cancellationToken);
        var items = await filtered.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((page - 1) * 20).Take(20)
            .Select(x => new { x.Id, x.Title, x.Body, x.Category, x.TargetPath, x.CreatedAt, x.ReadAt })
            .ToListAsync(cancellationToken);
        return Ok(new { items, unreadCount, totalCount, page, pageSize = 20, asOf });
    }
    public record ReadRequest(bool IsRead);
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> Read(Guid id, ReadRequest request, CancellationToken cancellationToken)
    {
        var item = await db.UserNotifications.SingleOrDefaultAsync(x => x.Id == id && x.UserId == UserId, cancellationToken);
        if (item is null) return NotFound();
        item.ReadAt = request.IsRead ? item.ReadAt ?? DateTime.UtcNow : null;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
    public record ReadAllRequest(DateTime Through);
    [HttpPost("read-all")]
    public async Task<IActionResult> ReadAll(ReadAllRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var through = request.Through.ToUniversalTime() > now ? now : request.Through.ToUniversalTime();
        await db.UserNotifications.Where(x => x.UserId == UserId && x.ReadAt == null && x.CreatedAt <= through)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ReadAt, (DateTime?)now), cancellationToken);
        return NoContent();
    }
}

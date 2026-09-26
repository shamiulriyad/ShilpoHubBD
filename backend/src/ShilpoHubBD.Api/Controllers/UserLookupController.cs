using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

// Name search so team-member forms can pick a person instead of pasting a raw user ID.
// Returns names and roles only (no emails), and only to the research roles that manage teams.
[ApiController]
[Route("api/users/lookup")]
[Authorize(Roles = $"{RoleNames.HeritageInnovationHub},{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}")]
public class UserLookupController(ShilpoHubDbContext db) : ControllerBase
{
    public record UserLookupItem(Guid Id, string FullName, List<string> Roles);

    [HttpGet]
    public async Task<ActionResult<List<UserLookupItem>>> Search([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var term = search?.Trim();
        if (string.IsNullOrEmpty(term) || term.Length < 2)
        {
            return Ok(new List<UserLookupItem>());
        }

        var pattern = $"%{term}%";
        var users = await db.Users.AsNoTracking()
            .Where(u => u.IsActive && (EF.Functions.ILike(u.FullName, pattern) || EF.Functions.ILike(u.Email, pattern)))
            .OrderBy(u => u.FullName)
            .Take(20)
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);

        var ids = users.Select(u => u.Id).ToList();
        var roles = await db.UserRoles.AsNoTracking()
            .Where(r => ids.Contains(r.UserId))
            .Select(r => new { r.UserId, r.Role.Name })
            .ToListAsync(cancellationToken);

        return Ok(users.Select(u => new UserLookupItem(
            u.Id, u.FullName, roles.Where(r => r.UserId == u.Id).Select(r => r.Name).ToList())).ToList());
    }
}

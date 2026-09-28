using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Repositories;

public class ArtisanSupportRepository(ShilpoHubDbContext context) : IArtisanSupportRepository
{
    public Task<SupportOrganizationProfile?> GetOrganizationByUserAsync(Guid userId, CancellationToken ct) => context.SupportOrganizationProfiles.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId, ct);
    public Task<SupportOrganizationProfile?> GetOrganizationAsync(Guid id, CancellationToken ct) => context.SupportOrganizationProfiles.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<List<SupportOrganizationProfile>> GetOrganizationsAsync(CancellationToken ct) => context.SupportOrganizationProfiles.Include(x => x.User).OrderBy(x => x.Status).ThenBy(x => x.OrganizationName).ToListAsync(ct);
    public Task<bool> RegistrationNumberExistsAsync(string number, Guid? exceptId, CancellationToken ct) => context.SupportOrganizationProfiles.AnyAsync(x => x.RegistrationNumber.ToLower() == number.ToLower() && (!exceptId.HasValue || x.Id != exceptId), ct);

    public Task<ArtisanSupportCase?> GetCaseAsync(Guid id, CancellationToken ct) => CaseQuery().FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<List<ArtisanSupportCase>> GetCasesAsync(Guid userId, bool isAdmin, bool isGovernment, CancellationToken ct)
    {
        var query = CaseQuery();
        if (!isAdmin) query = isGovernment ? query.Where(x => x.OrganizationUserId == userId || x.OrganizationUserId == null) : query.Where(x => x.ArtisanUserId == userId);
        return query.OrderByDescending(x => x.IsFlagged || x.HasDispute).ThenByDescending(x => x.UpdatedAt).ToListAsync(ct);
    }
    private IQueryable<ArtisanSupportCase> CaseQuery() => context.ArtisanSupportCases
        .Include(x => x.Artisan).Include(x => x.Organization)
        .Include(x => x.Evidence).Include(x => x.MonitoringEntries)
        .Include(x => x.FinalReport).AsSplitQuery();

    public Task<List<User>> GetUsersInRoleAsync(string roleName, CancellationToken ct) => context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.UserRoles.Any(r => r.Role.Name == roleName)).OrderBy(x => x.FullName).ToListAsync(ct);
    public Task<User?> GetUserAsync(Guid id, CancellationToken ct) => context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddOrganizationAsync(SupportOrganizationProfile profile, CancellationToken ct) => await context.SupportOrganizationProfiles.AddAsync(profile, ct);
    public async Task AddCaseAsync(ArtisanSupportCase supportCase, CancellationToken ct) => await context.ArtisanSupportCases.AddAsync(supportCase, ct);
    public Task SaveChangesAsync(CancellationToken ct) => context.SaveChangesAsync(ct);
}

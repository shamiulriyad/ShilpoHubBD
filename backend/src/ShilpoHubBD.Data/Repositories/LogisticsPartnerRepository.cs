using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Logistics;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class LogisticsPartnerRepository : ILogisticsPartnerRepository
{
    private readonly ShilpoHubDbContext _context;

    public LogisticsPartnerRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LogisticsPartnerProfile profile, CancellationToken cancellationToken)
        => await _context.LogisticsPartnerProfiles.AddAsync(profile, cancellationToken);

    public void Remove(LogisticsPartnerProfile profile)
        => _context.LogisticsPartnerProfiles.Remove(profile);

    public Task<LogisticsPartnerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.LogisticsPartnerProfiles
            .Include(p => p.User)
            .Include(p => p.BaseDistrict)
            .Include(p => p.VerifiedBy)
            .Include(p => p.ServiceAreas).ThenInclude(a => a.District)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<LogisticsPartnerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => _context.LogisticsPartnerProfiles
            .Include(p => p.User)
            .Include(p => p.BaseDistrict)
            .Include(p => p.VerifiedBy)
            .Include(p => p.ServiceAreas).ThenInclude(a => a.District)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public async Task<(List<LogisticsPartnerProfile> Items, int TotalCount)> GetPagedAsync(
        LogisticsPartnerQueryParameters query, CancellationToken cancellationToken)
    {
        var profiles = _context.LogisticsPartnerProfiles
            .Include(p => p.ServiceAreas)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.VerificationStatus)
            && Enum.TryParse<LogisticsPartnerVerificationStatus>(query.VerificationStatus, true, out var status))
        {
            profiles = profiles.Where(p => p.VerificationStatus == status);
        }

        if (query.IsAcceptingRequests.HasValue)
        {
            profiles = profiles.Where(p => p.IsAcceptingRequests == query.IsAcceptingRequests.Value);
        }

        if (query.ServiceDistrictId.HasValue)
        {
            profiles = profiles.Where(p => p.ServiceAreas.Any(a => a.DistrictId == query.ServiceDistrictId.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            profiles = profiles.Where(p =>
                p.CompanyName.ToLower().Contains(term)
                || p.BaseCity.ToLower().Contains(term)
                || p.ContactPersonName.ToLower().Contains(term));
        }

        profiles = profiles.OrderByDescending(p => p.CreatedAt);

        var totalCount = await profiles.CountAsync(cancellationToken);
        var items = await profiles
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> HasPickupRequestsAsync(Guid profileId, CancellationToken cancellationToken)
        => _context.PickupRequests.AnyAsync(r => r.LogisticsPartnerProfileId == profileId, cancellationToken);

    public Task<District?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.FirstOrDefaultAsync(d => d.Id == districtId, cancellationToken);

    public Task<bool> UserInRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken)
        => _context.Users.AnyAsync(
            u => u.Id == userId && u.UserRoles.Any(ur => ur.Role.Name == roleName),
            cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

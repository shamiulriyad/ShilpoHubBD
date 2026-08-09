using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.BusinessPartner;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Data.Repositories;

public class BusinessPartnerRepository : IBusinessPartnerRepository
{
    private readonly ShilpoHubDbContext _context;

    public BusinessPartnerRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<BusinessPartnerProfile> WithDetails()
        => _context.BusinessPartnerProfiles
            .Include(b => b.User)
            .Include(b => b.VerifiedBy)
            .Include(b => b.District)
            .Include(b => b.Documents)
            .Include(b => b.PreferredCategories).ThenInclude(c => c.Category)
            .AsSplitQuery();

    public Task<BusinessPartnerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);

    public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => _context.BusinessPartnerProfiles.AnyAsync(b => b.UserId == userId, cancellationToken);

    public Task<bool> ExistsByRegistrationNumberAsync(string registrationNumber, Guid? excludeProfileId, CancellationToken cancellationToken)
        => _context.BusinessPartnerProfiles.AnyAsync(
            b => b.RegistrationNumber == registrationNumber && (!excludeProfileId.HasValue || b.Id != excludeProfileId.Value),
            cancellationToken);

    public async Task<(List<BusinessPartnerProfile> Items, int TotalCount)> GetPagedAsync(
        BusinessPartnerQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = WithDetails();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(b =>
                EF.Functions.ILike(b.CompanyName, $"%{search}%") ||
                EF.Functions.ILike(b.Industry, $"%{search}%"));
        }

        if (parameters.BusinessType.HasValue)
        {
            query = query.Where(b => b.BusinessType == parameters.BusinessType.Value);
        }

        if (parameters.VerificationStatus.HasValue)
        {
            query = query.Where(b => b.VerificationStatus == parameters.VerificationStatus.Value);
        }

        if (parameters.DistrictId.HasValue)
        {
            query = query.Where(b => b.DistrictId == parameters.DistrictId.Value);
        }

        query = query.OrderByDescending(b => b.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(BusinessPartnerProfile profile, CancellationToken cancellationToken)
        => await _context.BusinessPartnerProfiles.AddAsync(profile, cancellationToken);

    public void Remove(BusinessPartnerProfile profile)
        => _context.BusinessPartnerProfiles.Remove(profile);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

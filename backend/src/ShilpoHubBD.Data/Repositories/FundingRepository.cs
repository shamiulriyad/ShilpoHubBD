using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Repositories;

public class FundingRepository : IFundingRepository
{
    private readonly ShilpoHubDbContext _context;

    public FundingRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // ---- Programs -------------------------------------------------------

    public async Task AddProgramAsync(FundingProgram program, CancellationToken cancellationToken)
        => await _context.FundingPrograms.AddAsync(program, cancellationToken);

    public void RemoveProgram(FundingProgram program) => _context.FundingPrograms.Remove(program);

    public Task<FundingProgram?> GetProgramByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.FundingPrograms
            .Include(p => p.ManagedBy)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ProgramSlugExistsAsync(string slug, CancellationToken cancellationToken)
        => _context.FundingPrograms.AnyAsync(p => p.Slug == slug, cancellationToken);

    public async Task<(List<FundingProgram> Items, int TotalCount)> GetProgramsPagedAsync(
        FundingProgramQueryParameters query, CancellationToken cancellationToken)
    {
        var programs = _context.FundingPrograms
            .Include(p => p.ManagedBy)
            .AsQueryable();

        if (TryEnum<FundingProgramType>(query.Type, out var type))
        {
            programs = programs.Where(p => p.Type == type);
        }

        if (TryEnum<FundingProgramStatus>(query.Status, out var status))
        {
            programs = programs.Where(p => p.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            programs = programs.Where(p => p.Name.ToLower().Contains(term) || p.Slug.ToLower().Contains(term));
        }

        programs = programs.OrderByDescending(p => p.CreatedAt);

        var totalCount = await programs.CountAsync(cancellationToken);
        var items = await programs
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(int ApplicationCount, int ApprovedCount)> GetProgramCountsAsync(
        Guid programId, CancellationToken cancellationToken)
    {
        var applications = _context.FundingApplications.Where(a => a.FundingProgramId == programId);
        var total = await applications.CountAsync(cancellationToken);
        var approved = await applications.CountAsync(
            a => a.Status == FundingApplicationStatus.Approved
                || a.Status == FundingApplicationStatus.Disbursing
                || a.Status == FundingApplicationStatus.Completed,
            cancellationToken);
        return (total, approved);
    }

    public async Task<Dictionary<Guid, int>> GetApplicationCountsAsync(
        IEnumerable<Guid> programIds, CancellationToken cancellationToken)
    {
        var ids = programIds.Distinct().ToList();
        return await _context.FundingApplications
            .Where(a => ids.Contains(a.FundingProgramId))
            .GroupBy(a => a.FundingProgramId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
    }

    // ---- Applications ---------------------------------------------

    public async Task AddApplicationAsync(FundingApplication application, CancellationToken cancellationToken)
        => await _context.FundingApplications.AddAsync(application, cancellationToken);

    public void RemoveApplication(FundingApplication application)
        => _context.FundingApplications.Remove(application);

    public Task<FundingApplication?> GetApplicationByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.FundingApplications
            .Include(a => a.Program)
            .Include(a => a.ApplicantUser)
            .Include(a => a.ApplicantVillage)
            .Include(a => a.DecisionBy)
            .Include(a => a.Reviews).ThenInclude(r => r.Reviewer)
            .Include(a => a.Disbursements).ThenInclude(d => d.RecordedBy)
            .Include(a => a.Events).ThenInclude(e => e.Actor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<bool> ApplicationReferenceExistsAsync(string referenceCode, CancellationToken cancellationToken)
        => _context.FundingApplications.AnyAsync(a => a.ReferenceCode == referenceCode, cancellationToken);

    public async Task<(List<FundingApplication> Items, int TotalCount)> GetApplicationsPagedAsync(
        FundingApplicationQueryParameters query, CancellationToken cancellationToken)
    {
        var applications = _context.FundingApplications
            .Include(a => a.Program)
            .AsQueryable();

        if (query.FundingProgramId.HasValue)
        {
            applications = applications.Where(a => a.FundingProgramId == query.FundingProgramId.Value);
        }

        if (TryEnum<FundingApplicationStatus>(query.Status, out var status))
        {
            applications = applications.Where(a => a.Status == status);
        }

        if (TryEnum<FundingApplicantType>(query.ApplicantType, out var applicantType))
        {
            applications = applications.Where(a => a.ApplicantType == applicantType);
        }

        if (query.ApplicantUserId.HasValue)
        {
            applications = applications.Where(a => a.ApplicantUserId == query.ApplicantUserId.Value);
        }

        if (query.ApplicantVillageId.HasValue)
        {
            applications = applications.Where(a => a.ApplicantVillageId == query.ApplicantVillageId.Value);
        }

        if (TryEnum<LoanRepaymentStatus>(query.RepaymentStatus, out var repaymentStatus))
        {
            applications = applications.Where(a => a.RepaymentStatus == repaymentStatus);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            applications = applications.Where(a =>
                a.ReferenceCode.ToLower().Contains(term)
                || a.ApplicantLabel.ToLower().Contains(term)
                || a.Purpose.ToLower().Contains(term));
        }

        applications = applications
            .OrderByDescending(a => a.Status == FundingApplicationStatus.Submitted
                || a.Status == FundingApplicationStatus.UnderReview)
            .ThenByDescending(a => a.SubmittedAt);

        var totalCount = await applications.CountAsync(cancellationToken);
        var items = await applications
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        => _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);

    public Task<bool> VillageExistsAsync(Guid villageId, CancellationToken cancellationToken)
        => _context.Villages.AnyAsync(v => v.Id == villageId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);

    private static bool TryEnum<T>(string? value, out T result) where T : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value, true, out result))
        {
            return true;
        }

        result = default;
        return false;
    }
}

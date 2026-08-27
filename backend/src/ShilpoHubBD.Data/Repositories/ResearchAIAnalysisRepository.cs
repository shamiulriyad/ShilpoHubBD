using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Repositories;

public class ResearchAIAnalysisRepository : IResearchAIAnalysisRepository
{
    private readonly ShilpoHubDbContext _context;

    public ResearchAIAnalysisRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<ResearchAIAnalysis?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.ResearchAIAnalyses
            .Include(a => a.RequestedBy)
            .Include(a => a.Dataset)
            .Include(a => a.Paper)
            .Include(a => a.Findings)
            .Include(a => a.Citations)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(List<ResearchAIAnalysis> Items, int TotalCount)> GetPagedForProjectAsync(
        Guid projectId, ResearchAIAnalysisQueryParameters query, CancellationToken cancellationToken)
    {
        var analyses = _context.ResearchAIAnalyses
            .Include(a => a.RequestedBy)
            .Include(a => a.Findings)
            .Include(a => a.Citations)
            .AsSplitQuery()
            .Where(a => a.ResearchProjectId == projectId);

        if (!string.IsNullOrWhiteSpace(query.Type)
            && Enum.TryParse<ResearchAIAnalysisType>(query.Type, true, out var type))
        {
            analyses = analyses.Where(a => a.AnalysisType == type);
        }

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<ResearchAIAnalysisStatus>(query.Status, true, out var status))
        {
            analyses = analyses.Where(a => a.Status == status);
        }

        analyses = analyses.OrderByDescending(a => a.CreatedAt);

        var totalCount = await analyses.CountAsync(cancellationToken);
        var items = await analyses
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(ResearchAIAnalysis analysis, CancellationToken cancellationToken)
        => await _context.ResearchAIAnalyses.AddAsync(analysis, cancellationToken);

    public void Remove(ResearchAIAnalysis analysis)
        => _context.ResearchAIAnalyses.Remove(analysis);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

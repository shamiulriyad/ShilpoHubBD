using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Portfolio;

namespace ShilpoHubBD.Data.Repositories;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly ShilpoHubDbContext _context;

    public PortfolioRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Portfolio> WithDetails()
        => _context.Portfolios
            .Include(p => p.Projects).ThenInclude(pr => pr.HeritageSkill)
            .AsSplitQuery();

    public Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Portfolio?> GetByAcademyMemberProfileIdAsync(Guid academyMemberProfileId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.AcademyMemberProfileId == academyMemberProfileId, cancellationToken);

    public async Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken)
        => await _context.Portfolios.AddAsync(portfolio, cancellationToken);

    public Task<PortfolioProject?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken)
        => _context.PortfolioProjects.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

    public async Task AddProjectAsync(PortfolioProject project, CancellationToken cancellationToken)
        => await _context.PortfolioProjects.AddAsync(project, cancellationToken);

    public void RemoveProject(PortfolioProject project)
        => _context.PortfolioProjects.Remove(project);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}

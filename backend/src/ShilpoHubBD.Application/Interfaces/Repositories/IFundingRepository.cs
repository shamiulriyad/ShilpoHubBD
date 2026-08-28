using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IFundingRepository
{
    // ---- Programs -----------------------------------------------------
    Task AddProgramAsync(FundingProgram program, CancellationToken cancellationToken);

    void RemoveProgram(FundingProgram program);

    Task<FundingProgram?> GetProgramByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ProgramSlugExistsAsync(string slug, CancellationToken cancellationToken);

    Task<(List<FundingProgram> Items, int TotalCount)> GetProgramsPagedAsync(
        FundingProgramQueryParameters query, CancellationToken cancellationToken);

    Task<(int ApplicationCount, int ApprovedCount)> GetProgramCountsAsync(
        Guid programId, CancellationToken cancellationToken);

    Task<Dictionary<Guid, int>> GetApplicationCountsAsync(
        IEnumerable<Guid> programIds, CancellationToken cancellationToken);

    // ---- Applications ----------------------------------------------
    Task AddApplicationAsync(FundingApplication application, CancellationToken cancellationToken);

    void RemoveApplication(FundingApplication application);

    Task<FundingApplication?> GetApplicationByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ApplicationReferenceExistsAsync(string referenceCode, CancellationToken cancellationToken);

    Task<(List<FundingApplication> Items, int TotalCount)> GetApplicationsPagedAsync(
        FundingApplicationQueryParameters query, CancellationToken cancellationToken);

    // ---- Cross-checks ------------------------------------------
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> VillageExistsAsync(Guid villageId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

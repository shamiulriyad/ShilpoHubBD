using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IArtisanSupportRepository
{
    Task<SupportOrganizationProfile?> GetOrganizationByUserAsync(Guid userId, CancellationToken ct);
    Task<SupportOrganizationProfile?> GetOrganizationAsync(Guid id, CancellationToken ct);
    Task<List<SupportOrganizationProfile>> GetOrganizationsAsync(CancellationToken ct);
    Task<bool> RegistrationNumberExistsAsync(string registrationNumber, Guid? exceptId, CancellationToken ct);
    Task<ArtisanSupportCase?> GetCaseAsync(Guid id, CancellationToken ct);
    Task<List<ArtisanSupportCase>> GetCasesAsync(Guid userId, bool isAdmin, bool isGovernment, CancellationToken ct);

    /// <summary>The most recently updated case per artisan, for the given set of artisan ids — admin-facing summary use, not access-controlled case viewing. Avoids N+1 lookups when annotating a list of producers.</summary>
    Task<Dictionary<Guid, ArtisanSupportCase>> GetLatestCasesForArtisansAsync(IEnumerable<Guid> artisanUserIds, CancellationToken ct);
    Task<List<User>> GetUsersInRoleAsync(string roleName, CancellationToken ct);
    Task<User?> GetUserAsync(Guid id, CancellationToken ct);
    Task AddOrganizationAsync(SupportOrganizationProfile profile, CancellationToken ct);
    Task AddCaseAsync(ArtisanSupportCase supportCase, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);

    /// <summary>With Metrics loaded, tracked for in-place updates.</summary>
    Task<ArtisanSupportImpactAssessment?> GetImpactAssessmentByCaseIdAsync(Guid caseId, CancellationToken ct);

    Task AddImpactAssessmentAsync(ArtisanSupportImpactAssessment assessment, CancellationToken ct);

    /// <summary>The most recent AI narrative for this case, with Findings loaded. Null if none has been generated yet.</summary>
    Task<ProducerImpactAIAnalysis?> GetLatestAiAnalysisByCaseIdAsync(Guid caseId, CancellationToken ct);

    Task AddAiAnalysisAsync(ProducerImpactAIAnalysis analysis, CancellationToken ct);
}

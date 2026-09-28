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
    Task<List<User>> GetUsersInRoleAsync(string roleName, CancellationToken ct);
    Task<User?> GetUserAsync(Guid id, CancellationToken ct);
    Task AddOrganizationAsync(SupportOrganizationProfile profile, CancellationToken ct);
    Task AddCaseAsync(ArtisanSupportCase supportCase, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

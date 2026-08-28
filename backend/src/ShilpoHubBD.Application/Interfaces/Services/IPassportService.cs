using ShilpoHubBD.Application.DTOs.Passport;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IPassportService
{
    Task<List<BadgeDto>> GetAllBadgesAsync(CancellationToken cancellationToken);
    Task<List<UserBadgeDto>> GetMyBadgesAsync(Guid userId, CancellationToken cancellationToken);
    Task<BadgeDto> CreateBadgeAsync(CreateBadgeRequest request, CancellationToken cancellationToken);
    Task<UserBadgeDto> ClaimDistrictBadgeAsync(Guid userId, ClaimDistrictBadgeRequest request, CancellationToken cancellationToken);
    Task<UserBadgeDto> ClaimFestivalBadgeAsync(Guid userId, ClaimFestivalBadgeRequest request, CancellationToken cancellationToken);
    Task<List<UserBadgeDto>> EvaluatePurchaseBadgesAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<UserBadgeDto>> EvaluateCheckInBadgesAsync(Guid userId, CancellationToken cancellationToken);

    Task<CheckInDto> CheckInAsync(Guid userId, CreateCheckInRequest request, CancellationToken cancellationToken);
    Task<List<CheckInDto>> GetMyCheckInsAsync(Guid userId, CancellationToken cancellationToken);

    Task<TravelJournalEntryDto> AddJournalEntryAsync(Guid userId, CreateJournalEntryRequest request, CancellationToken cancellationToken);
    Task<List<TravelJournalEntryDto>> GetMyJournalAsync(Guid userId, CancellationToken cancellationToken);
    Task<TravelJournalEntryDto> UpdateJournalEntryAsync(Guid userId, Guid id, UpdateJournalEntryRequest request, CancellationToken cancellationToken);
    Task DeleteJournalEntryAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}

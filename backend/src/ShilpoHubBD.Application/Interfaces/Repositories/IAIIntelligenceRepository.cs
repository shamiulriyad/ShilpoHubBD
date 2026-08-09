using ShilpoHubBD.Application.DTOs.AIIntelligence;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IAIIntelligenceRepository
{
    Task<ProducerIntelligenceProfileDto?> GetProducerIntelligenceProfileAsync(Guid producerId, CancellationToken cancellationToken);
    Task<List<PeriodPriceDto>> GetCategoryMonthlyAveragePriceAsync(Guid categoryId, int months, CancellationToken cancellationToken);
}

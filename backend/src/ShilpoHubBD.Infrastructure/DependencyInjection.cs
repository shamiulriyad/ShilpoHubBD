using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.AIBusiness;
using ShilpoHubBD.Infrastructure.AIBusinessPartner;
using ShilpoHubBD.Infrastructure.AILogistics;
using ShilpoHubBD.Infrastructure.AITourism;
using ShilpoHubBD.Infrastructure.Email;
using ShilpoHubBD.Infrastructure.GovForecasting;
using ShilpoHubBD.Infrastructure.HeritageIntelligence;
using ShilpoHubBD.Infrastructure.Options;
using ShilpoHubBD.Infrastructure.PolicySimulation;
using ShilpoHubBD.Infrastructure.Payments;
using ShilpoHubBD.Infrastructure.Recommendations;
using ShilpoHubBD.Infrastructure.ResearchAI;
using ShilpoHubBD.Infrastructure.Security;

namespace ShilpoHubBD.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IEmailSender, ConsoleEmailSender>();
        services.AddScoped<IPaymentProvider, CashOnDeliveryPaymentProvider>();
        services.AddScoped<IRecommendationProvider, DummyRecommendationProvider>();
        services.AddScoped<IAIBusinessProvider, DummyAIBusinessProvider>();
        services.AddScoped<IAIBusinessPartnerProvider, DummyBusinessPartnerAIProvider>();
        services.AddScoped<IAITourismProvider, DummyAITourismProvider>();
        services.AddScoped<IResearchAIProvider, DummyResearchAIProvider>();
        services.AddScoped<IHeritageIntelligenceProvider, RuleBasedHeritageIntelligenceProvider>();
        services.AddScoped<IPolicySimulationProvider, RuleBasedPolicySimulationProvider>();
        services.AddScoped<IGovForecastProvider, RuleBasedGovForecastProvider>();

        services.AddScoped<IDeliveryPredictionProvider, RuleBasedDeliveryPredictionProvider>();
        services.AddScoped<IAiRouteOptimizationProvider, RuleBasedAiRouteOptimizationProvider>();
        services.AddScoped<IDemandForecastProvider, RuleBasedDemandForecastProvider>();
        services.AddScoped<IWarehouseAllocationProvider, RuleBasedWarehouseAllocationProvider>();

        return services;
    }
}

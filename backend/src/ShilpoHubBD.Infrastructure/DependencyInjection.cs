using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.AIBusiness;
using ShilpoHubBD.Infrastructure.AIBusinessPartner;
using ShilpoHubBD.Infrastructure.AILogistics;
using ShilpoHubBD.Infrastructure.AITourism;
using ShilpoHubBD.Infrastructure.CounterfeitDetection;
using ShilpoHubBD.Infrastructure.Email;
using ShilpoHubBD.Infrastructure.Geocoding;
using ShilpoHubBD.Infrastructure.GovForecasting;
using ShilpoHubBD.Infrastructure.HeritageAssistant;
using ShilpoHubBD.Infrastructure.HeritageIntelligence;
using ShilpoHubBD.Infrastructure.Options;
using ShilpoHubBD.Infrastructure.PolicySimulation;
using ShilpoHubBD.Infrastructure.Payments;
using ShilpoHubBD.Infrastructure.Recommendations;
using ShilpoHubBD.Infrastructure.ResearchAI;
using ShilpoHubBD.Infrastructure.Routing;
using ShilpoHubBD.Infrastructure.Security;
using ShilpoHubBD.Infrastructure.SentimentAnalysis;
using ShilpoHubBD.Infrastructure.StoryGenerator;

namespace ShilpoHubBD.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<RagServiceOptions>(configuration.GetSection("RagService"));
        services.Configure<ShilpoHubBD.Infrastructure.ProductSearch.ProductSearchServiceOptions>(configuration.GetSection("ProductSearch"));
        services.Configure<GeminiOptions>(configuration.GetSection("Gemini"));
        services.Configure<NominatimOptions>(configuration.GetSection("Nominatim"));
        services.Configure<OsrmOptions>(configuration.GetSection("Osrm"));

        services.AddMemoryCache();
        services.AddSingleton<NominatimRateGate>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IEmailSender, ConsoleEmailSender>();
        services.AddScoped<IPaymentProvider, CashOnDeliveryPaymentProvider>();
        services.AddScoped<IRecommendationProvider, DummyRecommendationProvider>();
        services.AddScoped<IAIBusinessProvider, DummyAIBusinessProvider>();
        services.AddScoped<IAIBusinessPartnerProvider, DummyBusinessPartnerAIProvider>();
        services.AddScoped<DummyAITourismProvider>();
        services.AddHttpClient<IAITourismProvider, GeminiAITourismProvider>((sp, client) =>
        {
            var geminiOptions = sp.GetRequiredService<IOptions<GeminiOptions>>().Value;
            client.BaseAddress = new Uri(geminiOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(geminiOptions.TimeoutSeconds);
        });
        services.AddHttpClient<IGeocodingProvider, NominatimGeocodingProvider>((sp, client) =>
        {
            var nominatimOptions = sp.GetRequiredService<IOptions<NominatimOptions>>().Value;
            client.BaseAddress = new Uri(nominatimOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(nominatimOptions.TimeoutSeconds);
        });
        services.AddHttpClient<IRoutingProvider, OsrmRoutingProvider>((sp, client) =>
        {
            var osrmOptions = sp.GetRequiredService<IOptions<OsrmOptions>>().Value;
            client.BaseAddress = new Uri(osrmOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(osrmOptions.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(osrmOptions.UserAgent);
        });
        services.AddScoped<IResearchAIProvider, DummyResearchAIProvider>();
        services.AddScoped<IHeritageIntelligenceProvider, RuleBasedHeritageIntelligenceProvider>();
        services.AddScoped<IPolicySimulationProvider, RuleBasedPolicySimulationProvider>();
        services.AddScoped<IGovForecastProvider, RuleBasedGovForecastProvider>();

        services.AddScoped<IDeliveryPredictionProvider, RuleBasedDeliveryPredictionProvider>();
        services.AddScoped<IAiRouteOptimizationProvider, RuleBasedAiRouteOptimizationProvider>();
        services.AddScoped<IDemandForecastProvider, RuleBasedDemandForecastProvider>();
        services.AddScoped<IWarehouseAllocationProvider, RuleBasedWarehouseAllocationProvider>();

        services.AddScoped<IBackupRunner, PgDumpBackupRunner>();

        services.AddHttpClient<IHeritageAssistantProvider, RagHeritageAssistantProvider>((sp, client) =>
        {
            var ragOptions = sp.GetRequiredService<IOptions<RagServiceOptions>>().Value;
            client.BaseAddress = new Uri(ragOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(ragOptions.TimeoutSeconds);
        });
        services.AddHttpClient<ITravelPlannerRagProvider, RagTravelPlannerProvider>((sp, client) =>
        {
            var ragOptions = sp.GetRequiredService<IOptions<RagServiceOptions>>().Value;
            client.BaseAddress = new Uri(ragOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(ragOptions.TimeoutSeconds);
        });
        services.AddHttpClient<IProductSearchCandidateProvider, ShilpoHubBD.Infrastructure.ProductSearch.PythonProductSearchProvider>();
        services.AddHttpClient<IProductAttributeSuggester, ShilpoHubBD.Infrastructure.ProductSearch.PythonAttributeSuggester>();
        services.AddScoped<ICounterfeitDetectionProvider, RuleBasedCounterfeitDetectionProvider>();
        services.AddScoped<IStoryGeneratorProvider, RuleBasedStoryGeneratorProvider>();
        services.AddScoped<ISentimentAnalysisProvider, RuleBasedSentimentAnalysisProvider>();

        return services;
    }
}

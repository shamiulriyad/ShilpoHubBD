using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.AIBusiness;
using ShilpoHubBD.Infrastructure.Email;
using ShilpoHubBD.Infrastructure.Options;
using ShilpoHubBD.Infrastructure.Payments;
using ShilpoHubBD.Infrastructure.Recommendations;
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

        return services;
    }
}

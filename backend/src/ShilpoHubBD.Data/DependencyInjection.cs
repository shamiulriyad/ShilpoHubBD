using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Data.Repositories;

namespace ShilpoHubBD.Data;

public static class DependencyInjection
{
	public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection");
		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new InvalidOperationException(
				"Connection string 'DefaultConnection' is not configured. Set ConnectionStrings__DefaultConnection " +
				"to your Supabase Postgres connection string.");
		}

		services.AddDbContext<ShilpoHubDbContext>(options => options.UseNpgsql(connectionString));

		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IRoleRepository, RoleRepository>();
		services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
		services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

		return services;
	}
}

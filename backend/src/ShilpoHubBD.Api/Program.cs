using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShilpoHubBD.Api.Hubs;
using ShilpoHubBD.Api.Middlewares;
using ShilpoHubBD.Api.Realtime;
using ShilpoHubBD.Application;
using ShilpoHubBD.Application.DTOs.Auth;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Data;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Load a local .env file (repo root or backend/.env) for Development so
// ConnectionStrings__DefaultConnection / Supabase__* can be set without editing appsettings.json.
if (builder.Environment.IsDevelopment())
{
	var envCandidates = new[]
	{
		Path.Combine(builder.Environment.ContentRootPath, ".env"),
		Path.Combine(builder.Environment.ContentRootPath, "..", "..", ".env"),
		Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", ".env"),
	};

	var envFile = envCandidates.FirstOrDefault(File.Exists);
	if (envFile is not null)
	{
		DotNetEnv.Env.Load(envFile);
		builder.Configuration.AddEnvironmentVariables();
	}
}

// Add services to the container.
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
	.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(
		new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Enter a valid JWT access token.",
	});
	options.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
			},
			Array.Empty<string>()
		},
	});
});

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddData(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddScoped<IMessageNotifier, SignalRMessageNotifier>();
builder.Services.AddScoped<ILiveEventNotifier, SignalRLiveEventNotifier>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
	throw new InvalidOperationException(
		"Connection string 'DefaultConnection' is not configured. Set ConnectionStrings__DefaultConnection " +
		"(env var) or ConnectionStrings:DefaultConnection (appsettings/.env) to your Supabase Postgres connection string.");
}

builder.Services
	.AddHealthChecks()
	.AddNpgSql(connectionString, name: "supabase-postgres");

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
	throw new InvalidOperationException(
		"JWT signing key is not configured. Set Jwt__Key (env var) or Jwt:Key (appsettings/.env).");
}

builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.MapInboundClaims = false;
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidateAudience = true,
			ValidAudience = builder.Configuration["Jwt:Audience"],
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
			ClockSkew = TimeSpan.Zero,
		};

		// Browsers cannot set an Authorization header on the WebSocket handshake, so SignalR
		// clients pass the access token as a query parameter instead.
		options.Events = new JwtBearerEvents
		{
			OnMessageReceived = context =>
			{
				var accessToken = context.Request.Query["access_token"];
				if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
				{
					context.Token = accessToken;
				}

				return Task.CompletedTask;
			},
		};
	});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", policy => policy
		.WithOrigins("http://localhost:5173", "https://localhost:5173")
		.AllowAnyHeader()
		.AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(_ => { });

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MessagingHub>("/hubs/messaging");
app.MapHub<LiveEventHub>("/hubs/live-events");
app.MapHealthChecks("/health/db");

// Idempotent SuperAdmin seed for local/dev environments; controlled entirely via .env, never baked into migrations.
var seedEmail = builder.Configuration["Seed:SuperAdminEmail"];
var seedPassword = builder.Configuration["Seed:SuperAdminPassword"];
if (!string.IsNullOrWhiteSpace(seedEmail) && !string.IsNullOrWhiteSpace(seedPassword))
{
	using var scope = app.Services.CreateScope();
	var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
	if (!await userRepository.AnyInRoleAsync(RoleNames.SuperAdmin, CancellationToken.None))
	{
		var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
		await authService.RegisterAsync(
			new RegisterRequest
			{
				Email = seedEmail,
				Password = seedPassword,
				ConfirmPassword = seedPassword,
				FullName = "Super Admin",
				Roles = new List<string> { RoleNames.SuperAdmin },
			},
			ipAddress: null,
			CancellationToken.None);
	}
}

app.Run();

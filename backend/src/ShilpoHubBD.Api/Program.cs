using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
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
	// Several modules reuse short, generic DTO names (e.g. UpdateMilestoneStatusRequest); Swashbuckle's
	// default schemaId is just the class name, so those collide across namespaces without this.
	options.CustomSchemaIds(type => type.FullName);
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
builder.Services.AddScoped<ILiveClassNotifier, SignalRLiveClassNotifier>();

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

// Rate limiting — blunts automated scraping/credential-stuffing. Requests are
// partitioned by authenticated user id when present, otherwise by client IP, so a
// single anonymous scraper cannot pull the whole catalog and one user cannot starve
// others. Runs after authentication so the user principal is available for the key.
static string RatePartitionKey(HttpContext ctx)
{
	var userId = ctx.User.FindFirst("sub")?.Value
		?? ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
	return !string.IsNullOrEmpty(userId)
		? $"user:{userId}"
		: $"ip:{ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
}

static string RateIpKey(HttpContext ctx) =>
	$"ip:{ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

	options.OnRejected = async (context, cancellationToken) =>
	{
		if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
		{
			context.HttpContext.Response.Headers.RetryAfter =
				((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
		}

		context.HttpContext.Response.ContentType = "application/json";
		await context.HttpContext.Response.WriteAsync(
			"{\"error\":\"Too many requests. Please slow down and try again shortly.\"}",
			cancellationToken);
	};

	// Applies to every endpoint: generous for signed-in users, tight for anonymous callers.
	options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
	{
		var authenticated = ctx.User.Identity?.IsAuthenticated == true;
		return RateLimitPartition.GetFixedWindowLimiter(RatePartitionKey(ctx), _ =>
			new FixedWindowRateLimiterOptions
			{
				PermitLimit = authenticated ? 300 : 75,
				Window = TimeSpan.FromMinutes(1),
			});
	});

	// Stricter policy for anonymous catalog/list ("browse") endpoints.
	options.AddPolicy("read", ctx =>
	{
		var authenticated = ctx.User.Identity?.IsAuthenticated == true;
		return RateLimitPartition.GetFixedWindowLimiter(RatePartitionKey(ctx), _ =>
			new FixedWindowRateLimiterOptions
			{
				PermitLimit = authenticated ? 200 : 40,
				Window = TimeSpan.FromMinutes(1),
			});
	});

	// Auth endpoints — always keyed by IP to limit credential stuffing / enumeration.
	options.AddPolicy("auth", ctx =>
		RateLimitPartition.GetFixedWindowLimiter(RateIpKey(ctx), _ =>
			new FixedWindowRateLimiterOptions
			{
				PermitLimit = 10,
				Window = TimeSpan.FromMinutes(1),
			}));
});

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

app.UseRateLimiter();

app.MapControllers();
app.MapHub<MessagingHub>("/hubs/messaging");
app.MapHub<LiveEventHub>("/hubs/live-events");
app.MapHub<LiveClassHub>("/hubs/live-classes");
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

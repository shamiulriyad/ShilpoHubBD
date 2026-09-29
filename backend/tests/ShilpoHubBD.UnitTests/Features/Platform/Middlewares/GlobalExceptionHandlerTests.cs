using System.Text.Json;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Api.Middlewares;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Domain.Entities.Security;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;
using FluentValidationException = FluentValidation.ValidationException;

namespace ShilpoHubBD.UnitTests.Features.Platform.Middlewares;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Platform")]
[Trait("Layer", "Middleware")]
public class GlobalExceptionHandlerTests
{
    private readonly TestDatabaseFixture _database;

    public GlobalExceptionHandlerTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static (GlobalExceptionHandler Handler, DefaultHttpContext Context, CapturingLogger<GlobalExceptionHandler> Logger) Create()
    {
        var logger = new CapturingLogger<GlobalExceptionHandler>();
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        context.Request.Method = "POST";
        context.Request.Path = "/api/orders";
        return (new GlobalExceptionHandler(logger), context, logger);
    }

    private static async Task<JsonDocument> BodyAsync(DefaultHttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return JsonDocument.Parse(await reader.ReadToEndAsync(Ct));
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400WithFieldErrorsGroupedByProperty()
    {
        var (handler, context, _) = Create();
        var errors = new FluentValidationException(new[]
        {
            new ValidationFailure("Email", "Email is required."),
            new ValidationFailure("Email", "Email is invalid."),
            new ValidationFailure("Password", "Password is required."),
        });

        var handled = await handler.TryHandleAsync(context, errors, Ct);

        Assert.True(handled);
        Assert.Equal(400, context.Response.StatusCode);
        var body = await BodyAsync(context);
        Assert.Equal("Validation failed.", body.RootElement.GetProperty("title").GetString());
        var fieldErrors = body.RootElement.GetProperty("errors");
        Assert.Equal(2, fieldErrors.GetProperty("Email").GetArrayLength());
        Assert.Equal(1, fieldErrors.GetProperty("Password").GetArrayLength());
        Assert.Equal("/api/orders", body.RootElement.GetProperty("instance").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_Returns409WithTheExceptionMessageAsTitle()
    {
        var (handler, context, _) = Create();

        await handler.TryHandleAsync(context, new ConflictException("Email is already registered."), Ct);

        Assert.Equal(409, context.Response.StatusCode);
        Assert.Equal("Email is already registered.", (await BodyAsync(context)).RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404()
    {
        var (handler, context, _) = Create();

        await handler.TryHandleAsync(context, new NotFoundException("User not found."), Ct);

        Assert.Equal(404, context.Response.StatusCode);
        Assert.Equal("User not found.", (await BodyAsync(context)).RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_AiServiceUnavailableException_Returns503()
    {
        var (handler, context, _) = Create();

        await handler.TryHandleAsync(context, new AiServiceUnavailableException("The AI service is temporarily unavailable."), Ct);

        Assert.Equal(503, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedAccessException_Returns401()
    {
        var (handler, context, _) = Create();

        await handler.TryHandleAsync(context, new UnauthorizedAccessException("You do not hold the requested role."), Ct);

        Assert.Equal(401, context.Response.StatusCode);
        Assert.Equal("You do not hold the requested role.", (await BodyAsync(context)).RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_UnrecognizedException_Returns500WithAFixedMessageThatDoesNotLeakTheRealOne()
    {
        var (handler, context, logger) = Create();

        await handler.TryHandleAsync(context, new InvalidOperationException("connection string contains a password"), Ct);

        Assert.Equal(500, context.Response.StatusCode);
        var title = (await BodyAsync(context)).RootElement.GetProperty("title").GetString();
        Assert.Equal("An unexpected error occurred.", title);
        Assert.DoesNotContain("password", title);
        Assert.Contains(logger.Entries, e => e.Level == Microsoft.Extensions.Logging.LogLevel.Error);
    }

    [Fact]
    public async Task TryHandleAsync_UnrecognizedException_LogsTheRequestMethodAndPath()
    {
        var (handler, context, logger) = Create();

        await handler.TryHandleAsync(context, new Exception("boom"), Ct);

        Assert.Contains(logger.Entries, e => e.Message.Contains("POST") && e.Message.Contains("/api/orders"));
    }

    [Fact]
    [Trait("Needs", "Database")]
    public async Task TryHandleAsync_DbUpdateConcurrencyException_LogsEachEntrysTypeStateAndKeys()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        // A Modified entry pointing at a row that never existed makes the UPDATE affect zero rows,
        // which EF Core reports as a genuine DbUpdateConcurrencyException -- no rowversion needed.
        var missingId = Guid.NewGuid();
        var log = new AuditLog { Id = missingId, ActorName = "x", Action = "x", EntityType = "x", Description = "x", CreatedAt = DateTime.UtcNow };
        context.Attach(log).State = EntityState.Modified;
        DbUpdateConcurrencyException? caught = null;
        try { await context.SaveChangesAsync(Ct); }
        catch (DbUpdateConcurrencyException ex) { caught = ex; }
        Assert.NotNull(caught);

        var (handler, httpContext, logger) = Create();
        await handler.TryHandleAsync(httpContext, caught, Ct);

        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Contains(logger.Entries, e => e.Message.Contains("DIAG concurrency entry") && e.Message.Contains("AuditLog") && e.Message.Contains($"Id={missingId}"));
    }
}

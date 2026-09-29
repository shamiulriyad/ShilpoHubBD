using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Data;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Notifications.Controllers;

/// <summary>NotificationsController queries ShilpoHubDbContext directly, so every test needs the test database.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Notifications")]
[Trait("Layer", "Controller")]
[Trait("Needs", "Database")]
public class NotificationsControllerTests
{
    private readonly TestDatabaseFixture _database;

    public NotificationsControllerTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static NotificationsController CreateController(ShilpoHubDbContext context)
        => (NotificationsController)Activator.CreateInstance(typeof(NotificationsController), context)!;

    [Fact]
    public void Controller_RequiresSignInUnderApiNotifications()
    {
        Assert.NotNull(typeof(NotificationsController).GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal("api/notifications", AccessRules.ControllerRoute(typeof(NotificationsController)));
    }

    [Theory]
    [InlineData(nameof(NotificationsController.List), "GET", null)]
    [InlineData(nameof(NotificationsController.Read), "PATCH", "{id:guid}/read")]
    [InlineData(nameof(NotificationsController.ReadAll), "POST", "read-all")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(NotificationsController), action));

    private static async Task<(User User, ShilpoHubDbContext Context)> SeedUserAsync(DatabaseScope db)
    {
        var user = TestUsers.Create();
        var context = db.NewContext();
        context.Users.Add(user);
        await context.SaveChangesAsync(Ct);
        return (user, db.NewContext());
    }

    private static JsonElement Body(IActionResult result)
    {
        var value = Assert.IsAssignableFrom<OkObjectResult>(result).Value;
        return JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement;
    }

    // ---------- List ----------

    [Fact]
    public async Task List_ReturnsOnlyTheSignedInUsersNotificationsNewestFirst()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        var other = TestUsers.Create();
        seedContext.Users.Add(other);
        seedContext.UserNotifications.AddRange(
            new UserNotification { UserId = user.Id, Title = "Older", Body = "b", CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new UserNotification { UserId = user.Id, Title = "Newer", Body = "b", CreatedAt = DateTime.UtcNow },
            new UserNotification { UserId = other.Id, Title = "Someone else's", Body = "b", CreatedAt = DateTime.UtcNow });
        await seedContext.SaveChangesAsync(Ct);

        var result = await CreateController(db.NewContext()).WithUser(user.Id).List(cancellationToken: Ct);

        var body = Body(result);
        var titles = body.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("Title").GetString()).ToList();
        Assert.Equal(new[] { "Newer", "Older" }, titles);
        Assert.Equal(2, body.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task List_ReportsHowManyAreUnreadRegardlessOfTheUnreadOnlyFilter()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        seedContext.UserNotifications.AddRange(
            new UserNotification { UserId = user.Id, Title = "Unread", Body = "b", CreatedAt = DateTime.UtcNow },
            new UserNotification { UserId = user.Id, Title = "Read", Body = "b", CreatedAt = DateTime.UtcNow, ReadAt = DateTime.UtcNow });
        await seedContext.SaveChangesAsync(Ct);

        var result = await CreateController(db.NewContext()).WithUser(user.Id).List(cancellationToken: Ct);

        Assert.Equal(1, (Body(result)).GetProperty("unreadCount").GetInt32());
    }

    [Fact]
    public async Task List_UnreadOnly_ReturnsOnlyUnreadItemsButKeepsTheFullUnreadCount()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        seedContext.UserNotifications.AddRange(
            new UserNotification { UserId = user.Id, Title = "Unread", Body = "b", CreatedAt = DateTime.UtcNow },
            new UserNotification { UserId = user.Id, Title = "Read", Body = "b", CreatedAt = DateTime.UtcNow, ReadAt = DateTime.UtcNow });
        await seedContext.SaveChangesAsync(Ct);

        var result = await CreateController(db.NewContext()).WithUser(user.Id).List(unreadOnly: true, cancellationToken: Ct);

        var body = Body(result);
        Assert.Equal("Unread", Assert.Single(body.GetProperty("items").EnumerateArray()).GetProperty("Title").GetString());
        Assert.Equal(1, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, body.GetProperty("unreadCount").GetInt32());
    }

    [Fact]
    public async Task List_FutureDatedNotification_IsNotShownUntilItsCreatedTimeArrives()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        seedContext.UserNotifications.Add(new UserNotification { UserId = user.Id, Title = "Future", Body = "b", CreatedAt = DateTime.UtcNow.AddMinutes(5) });
        await seedContext.SaveChangesAsync(Ct);

        var result = await CreateController(db.NewContext()).WithUser(user.Id).List(cancellationToken: Ct);

        Assert.Equal(0, (Body(result)).GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task List_SecondPage_SkipsTheFirstTwentyItems()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        var now = DateTime.UtcNow;
        seedContext.UserNotifications.AddRange(
            Enumerable.Range(0, 25).Select(i => new UserNotification { UserId = user.Id, Title = $"N{i}", Body = "b", CreatedAt = now.AddSeconds(-i) }));
        await seedContext.SaveChangesAsync(Ct);

        var result = await CreateController(db.NewContext()).WithUser(user.Id).List(page: 2, cancellationToken: Ct);

        var body = Body(result);
        Assert.Equal(5, body.GetProperty("items").GetArrayLength());
        Assert.Equal(2, body.GetProperty("page").GetInt32());
    }

    [Fact]
    public async Task List_ReadsTheUserIdFromTheNameIdentifierOrSubClaim()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        seedContext.UserNotifications.Add(new UserNotification { UserId = user.Id, Title = "Via sub", Body = "b", CreatedAt = DateTime.UtcNow });
        await seedContext.SaveChangesAsync(Ct);
        var controller = CreateController(db.NewContext());
        controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim("sub", user.Id.ToString()) }, "Test")),
        };

        var result = await controller.List(cancellationToken: Ct);

        Assert.Equal(1, (Body(result)).GetProperty("totalCount").GetInt32());
    }

    // ---------- Read ----------

    [Fact]
    public async Task Read_MarkAsRead_SetsReadAtAndReturnsNoContent()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        var notification = new UserNotification { UserId = user.Id, Title = "n", Body = "b", CreatedAt = DateTime.UtcNow };
        seedContext.UserNotifications.Add(notification);
        await seedContext.SaveChangesAsync(Ct);

        var result = await CreateController(db.NewContext()).WithUser(user.Id)
            .Read(notification.Id, new NotificationsController.ReadRequest(true), Ct);

        Assert.IsType<NoContentResult>(result);
        Assert.NotNull((await db.NewContext().UserNotifications.SingleAsync(n => n.Id == notification.Id, Ct)).ReadAt);
    }

    [Fact]
    public async Task Read_MarkAsReadTwice_KeepsTheOriginalReadTimestamp()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        var readAt = DateTime.UtcNow.AddHours(-1);
        var notification = new UserNotification { UserId = user.Id, Title = "n", Body = "b", CreatedAt = DateTime.UtcNow, ReadAt = readAt };
        seedContext.UserNotifications.Add(notification);
        await seedContext.SaveChangesAsync(Ct);

        await CreateController(db.NewContext()).WithUser(user.Id).Read(notification.Id, new NotificationsController.ReadRequest(true), Ct);

        // Postgres timestamps round-trip at microsecond precision, one tick coarser than .NET's;
        // a sub-microsecond difference here is storage rounding, not evidence the value changed.
        Assert.Equal(readAt, (await db.NewContext().UserNotifications.SingleAsync(n => n.Id == notification.Id, Ct)).ReadAt!.Value, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task Read_MarkAsUnread_ClearsReadAt()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        var notification = new UserNotification { UserId = user.Id, Title = "n", Body = "b", CreatedAt = DateTime.UtcNow, ReadAt = DateTime.UtcNow };
        seedContext.UserNotifications.Add(notification);
        await seedContext.SaveChangesAsync(Ct);

        await CreateController(db.NewContext()).WithUser(user.Id).Read(notification.Id, new NotificationsController.ReadRequest(false), Ct);

        Assert.Null((await db.NewContext().UserNotifications.SingleAsync(n => n.Id == notification.Id, Ct)).ReadAt);
    }

    [Fact]
    public async Task Read_UnknownId_ReturnsNotFound()
    {
        await using var db = await _database.BeginAsync();
        var (user, _) = await SeedUserAsync(db);

        var result = await CreateController(db.NewContext()).WithUser(user.Id)
            .Read(Guid.NewGuid(), new NotificationsController.ReadRequest(true), Ct);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Read_NotificationBelongsToAnotherUser_ReturnsNotFound()
    {
        await using var db = await _database.BeginAsync();
        var (owner, seedContext) = await SeedUserAsync(db);
        var stranger = TestUsers.Create();
        seedContext.Users.Add(stranger);
        var notification = new UserNotification { UserId = owner.Id, Title = "n", Body = "b", CreatedAt = DateTime.UtcNow };
        seedContext.UserNotifications.Add(notification);
        await seedContext.SaveChangesAsync(Ct);

        var result = await CreateController(db.NewContext()).WithUser(stranger.Id)
            .Read(notification.Id, new NotificationsController.ReadRequest(true), Ct);

        Assert.IsType<NotFoundResult>(result);
        Assert.Null((await db.NewContext().UserNotifications.SingleAsync(n => n.Id == notification.Id, Ct)).ReadAt);
    }

    // ---------- ReadAll ----------

    [Fact]
    public async Task ReadAll_MarksEveryUnreadNotificationUpToTheGivenTimeAsRead()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        var now = DateTime.UtcNow;
        var before = new UserNotification { UserId = user.Id, Title = "before", Body = "b", CreatedAt = now.AddMinutes(-2) };
        var after = new UserNotification { UserId = user.Id, Title = "after", Body = "b", CreatedAt = now.AddMinutes(2) };
        seedContext.UserNotifications.AddRange(before, after);
        await seedContext.SaveChangesAsync(Ct);

        var result = await CreateController(db.NewContext()).WithUser(user.Id)
            .ReadAll(new NotificationsController.ReadAllRequest(now), Ct);

        Assert.IsType<NoContentResult>(result);
        var check = db.NewContext();
        Assert.NotNull((await check.UserNotifications.SingleAsync(n => n.Id == before.Id, Ct)).ReadAt);
        Assert.Null((await check.UserNotifications.SingleAsync(n => n.Id == after.Id, Ct)).ReadAt);
    }

    [Fact]
    public async Task ReadAll_ThroughInTheFuture_IsClampedToNowSoLaterArrivalsStayUnread()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        var justNow = new UserNotification { UserId = user.Id, Title = "just now", Body = "b", CreatedAt = DateTime.UtcNow };
        seedContext.UserNotifications.Add(justNow);
        await seedContext.SaveChangesAsync(Ct);

        await CreateController(db.NewContext()).WithUser(user.Id)
            .ReadAll(new NotificationsController.ReadAllRequest(DateTime.UtcNow.AddDays(1)), Ct);

        Assert.NotNull((await db.NewContext().UserNotifications.SingleAsync(n => n.Id == justNow.Id, Ct)).ReadAt);
    }

    [Fact]
    public async Task ReadAll_DoesNotAffectAnotherUsersNotifications()
    {
        await using var db = await _database.BeginAsync();
        var (user, seedContext) = await SeedUserAsync(db);
        var other = TestUsers.Create();
        seedContext.Users.Add(other);
        var othersNotification = new UserNotification { UserId = other.Id, Title = "n", Body = "b", CreatedAt = DateTime.UtcNow };
        seedContext.UserNotifications.Add(othersNotification);
        await seedContext.SaveChangesAsync(Ct);

        await CreateController(db.NewContext()).WithUser(user.Id).ReadAll(new NotificationsController.ReadAllRequest(DateTime.UtcNow), Ct);

        Assert.Null((await db.NewContext().UserNotifications.SingleAsync(n => n.Id == othersNotification.Id, Ct)).ReadAt);
    }
}

using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Learning;
using ShilpoHubBD.Domain.Entities.Messaging;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Notifications.Data;

/// <summary>
/// Covers the notification-generation mechanism itself (SaveChangesAsync override), using a
/// representative sample of entity types rather than every branch: an explicit multi-recipient
/// branch (ArtisanSupportCase), a "notify everyone but the actor" branch (Message), the generic
/// status-based fallback (CourseEnrollment, via a Status property and a FK to User), the transactional
/// guarantee, and de-duplication. The full branch set is exercised end-to-end by the existing
/// backend/tests/NotificationsRegression console program.
/// </summary>
[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Notifications")]
[Trait("Layer", "Data")]
[Trait("Needs", "Database")]
public class ShilpoHubDbContextNotificationsTests
{
    private readonly TestDatabaseFixture _database;

    public ShilpoHubDbContextNotificationsTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>
    /// Seeds a SuperAdmin and commits it on its own, in a context the caller then discards. The
    /// admin lookups inside BuildNotificationsAsync query the database directly, so an admin only
    /// counts once their role assignment has actually been saved, not merely tracked.
    /// </summary>
    private static async Task<User> AddSuperAdminAsync(DatabaseScope db)
    {
        var context = db.NewContext();
        var role = await context.Roles.SingleAsync(r => r.Name == RoleNames.SuperAdmin, Ct);
        var admin = TestUsers.Create().WithRoles(role);
        context.Users.Add(admin);
        await context.SaveChangesAsync(Ct);
        return admin;
    }


    // ---------- generic status fallback (CourseEnrollment) ----------

    [Fact]
    public async Task SaveChangesAsync_NewStatusBearingEntity_NotifiesTheUserItReferences()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var apprentice = TestUsers.Create();
        var course = new Course { Id = Guid.NewGuid(), Title = "Jamdani Weaving", Description = "d", Category = "Weaving", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Users.Add(apprentice);
        context.Courses.Add(course);
        context.CourseEnrollments.Add(new CourseEnrollment { Id = Guid.NewGuid(), CourseId = course.Id, ApprenticeId = apprentice.Id, EnrolledAt = DateTime.UtcNow });

        await context.SaveChangesAsync(Ct);

        var notification = await db.NewContext().UserNotifications.SingleAsync(n => n.UserId == apprentice.Id, Ct);
        Assert.Equal("Learning", notification.Category);
        Assert.Equal("/dashboard/academy", notification.TargetPath);
        Assert.Contains("Course Enrollment", notification.Title);
        Assert.Null(notification.ReadAt);
    }

    [Fact]
    public async Task SaveChangesAsync_StatusUnchangedOnUpdate_CreatesNoNotification()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var apprentice = TestUsers.Create();
        var course = new Course { Id = Guid.NewGuid(), Title = "Jamdani Weaving", Description = "d", Category = "Weaving", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var enrollment = new CourseEnrollment { Id = Guid.NewGuid(), CourseId = course.Id, ApprenticeId = apprentice.Id, EnrolledAt = DateTime.UtcNow };
        context.Users.Add(apprentice);
        context.Courses.Add(course);
        context.CourseEnrollments.Add(enrollment);
        await context.SaveChangesAsync(Ct);

        var editContext = db.NewContext();
        var loaded = await editContext.CourseEnrollments.SingleAsync(e => e.Id == enrollment.Id, Ct);
        loaded.CompletedAt = DateTime.UtcNow;
        await editContext.SaveChangesAsync(Ct);

        Assert.False(await db.NewContext().UserNotifications.AnyAsync(n => n.UserId == apprentice.Id && n.Title.Contains("updated"), Ct));
    }

    // ---------- explicit multi-recipient branch (ArtisanSupportCase) ----------

    [Fact]
    public async Task SaveChangesAsync_NewArtisanSupportCase_NotifiesEverySuperAdminAndTheOrganization()
    {
        await using var db = await _database.BeginAsync();
        var admin = await AddSuperAdminAsync(db);
        var context = db.NewContext();
        var artisan = TestUsers.Create();
        var organization = TestUsers.Create();
        var creator = TestUsers.Create();
        context.Users.AddRange(artisan, organization, creator);
        context.ArtisanSupportCases.Add(new ArtisanSupportCase
        {
            Id = Guid.NewGuid(), CaseNumber = "CASE-0001", ArtisanUserId = artisan.Id, OrganizationUserId = organization.Id,
            CreatedByUserId = creator.Id, ProblemTitle = "Broken loom", ProblemDescription = "d", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });

        await context.SaveChangesAsync(Ct);

        var check = db.NewContext();
        // The admin also has a "Workspace access updated" notification from AddSuperAdminAsync
        // seeding their role in an earlier save, so match on title rather than take a single row.
        var adminNotification = await check.UserNotifications.SingleAsync(n => n.UserId == admin.Id && n.Title == "New artisan support case", Ct);
        Assert.Equal("/admin/artisan-support", adminNotification.TargetPath);
        var orgNotification = await check.UserNotifications.SingleAsync(n => n.UserId == organization.Id, Ct);
        Assert.Equal("Artisan case created", orgNotification.Title);
        Assert.False(await check.UserNotifications.AnyAsync(n => n.UserId == artisan.Id, Ct));
    }

    [Fact]
    public async Task SaveChangesAsync_ArtisanSupportCaseStatusChangedToClosed_NotifiesArtisanAndOrganizationButNotAdmins()
    {
        await using var db = await _database.BeginAsync();
        var admin = await AddSuperAdminAsync(db);
        var seedContext = db.NewContext();
        var artisan = TestUsers.Create();
        var organization = TestUsers.Create();
        var creator = TestUsers.Create();
        seedContext.Users.AddRange(artisan, organization, creator);
        var supportCase = new ArtisanSupportCase
        {
            Id = Guid.NewGuid(), CaseNumber = "CASE-0002", ArtisanUserId = artisan.Id, OrganizationUserId = organization.Id,
            CreatedByUserId = creator.Id, ProblemTitle = "t", ProblemDescription = "d", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        seedContext.ArtisanSupportCases.Add(supportCase);
        await seedContext.SaveChangesAsync(Ct);
        // The case's own creation already notified the admin ("New artisan support case") and the
        // admin's seeding already notified them ("Workspace access updated"); only the status-change
        // save below is under test, so its effect on the admin's notifications is measured by count,
        // not by which titles exist.
        var adminNotificationsBeforeClosing = await seedContext.UserNotifications.CountAsync(n => n.UserId == admin.Id, Ct);

        var editContext = db.NewContext();
        var loaded = await editContext.ArtisanSupportCases.SingleAsync(c => c.Id == supportCase.Id, Ct);
        loaded.Status = ArtisanSupportCaseStatus.Closed;
        await editContext.SaveChangesAsync(Ct);

        var check = db.NewContext();
        Assert.True(await check.UserNotifications.AnyAsync(n => n.UserId == artisan.Id && n.Title == "Support case updated", Ct));
        Assert.True(await check.UserNotifications.AnyAsync(n => n.UserId == organization.Id && n.Title == "Support case updated", Ct));
        Assert.Equal(adminNotificationsBeforeClosing, await check.UserNotifications.CountAsync(n => n.UserId == admin.Id, Ct));
    }

    // ---------- "everyone but the actor" branch (Message) ----------

    [Fact]
    public async Task SaveChangesAsync_NewMessage_NotifiesOtherParticipantsButNotTheSender()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var sender = TestUsers.Create();
        var recipient = TestUsers.Create();
        context.Users.AddRange(sender, recipient);
        var conversation = new Conversation { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Conversations.Add(conversation);
        context.ConversationParticipants.AddRange(
            new ConversationParticipant { Id = Guid.NewGuid(), ConversationId = conversation.Id, UserId = sender.Id },
            new ConversationParticipant { Id = Guid.NewGuid(), ConversationId = conversation.Id, UserId = recipient.Id });
        context.Messages.Add(new Message { Id = Guid.NewGuid(), ConversationId = conversation.Id, SenderId = sender.Id, Body = "Hello", CreatedAt = DateTime.UtcNow });

        await context.SaveChangesAsync(Ct);

        var check = db.NewContext();
        Assert.True(await check.UserNotifications.AnyAsync(n => n.UserId == recipient.Id && n.Title == "New message", Ct));
        Assert.False(await check.UserNotifications.AnyAsync(n => n.UserId == sender.Id, Ct));
    }

    // ---------- ignored entities ----------

    [Fact]
    public async Task SaveChangesAsync_EntityWithNoStatusOrSpecialHandling_CreatesNoNotification()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        context.Roles.Add(TestUsers.Role(TestUsers.UniqueRoleName()));
        var before = await context.UserNotifications.CountAsync(Ct);

        await context.SaveChangesAsync(Ct);

        Assert.Equal(before, await db.NewContext().UserNotifications.CountAsync(Ct));
    }

    // ---------- de-duplication ----------

    [Fact]
    public async Task SaveChangesAsync_TwoIdenticalNotificationsInOneSave_AreCollapsedToOne()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var sender = TestUsers.Create();
        var recipient = TestUsers.Create();
        context.Users.AddRange(sender, recipient);
        var conversation = new Conversation { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Conversations.Add(conversation);
        context.ConversationParticipants.AddRange(
            new ConversationParticipant { Id = Guid.NewGuid(), ConversationId = conversation.Id, UserId = sender.Id },
            new ConversationParticipant { Id = Guid.NewGuid(), ConversationId = conversation.Id, UserId = recipient.Id });
        // The "New message" notification body carries no per-message detail, so two messages sent
        // to the same conversation in one save both notify the recipient with the exact same
        // title/body/path, which the de-duplication in BuildNotificationsAsync should collapse to one.
        context.Messages.AddRange(
            new Message { Id = Guid.NewGuid(), ConversationId = conversation.Id, SenderId = sender.Id, Body = "Hello", CreatedAt = DateTime.UtcNow },
            new Message { Id = Guid.NewGuid(), ConversationId = conversation.Id, SenderId = sender.Id, Body = "Are you there?", CreatedAt = DateTime.UtcNow });

        await context.SaveChangesAsync(Ct);

        var count = await db.NewContext().UserNotifications.CountAsync(n => n.UserId == recipient.Id && n.Title == "New message", Ct);
        Assert.Equal(1, count);
    }

    // ---------- the notification is part of the same transaction as the activity ----------

    [Fact]
    public async Task SaveChangesAsync_ActivitySaveFails_NoNotificationIsPersisted()
    {
        await using var db = await _database.BeginAsync();
        await AddSuperAdminAsync(db);
        var context = db.NewContext();
        var artisan = TestUsers.Create();
        var creator = TestUsers.Create();
        context.Users.AddRange(artisan, creator);
        // Two different rows sharing a CaseNumber pass the change tracker's own identity check (their
        // primary keys differ) but violate the database's unique index, so the failure surfaces only
        // once Postgres runs the insert inside SaveChanges -- after notifications were added to the
        // change tracker but before they were committed.
        const string sharedCaseNumber = "FAIL-DUPLICATE";
        context.ArtisanSupportCases.Add(new ArtisanSupportCase
        {
            Id = Guid.NewGuid(), CaseNumber = sharedCaseNumber, ArtisanUserId = artisan.Id, CreatedByUserId = creator.Id,
            ProblemTitle = "t", ProblemDescription = "d", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        context.ArtisanSupportCases.Add(new ArtisanSupportCase
        {
            Id = Guid.NewGuid(), CaseNumber = sharedCaseNumber, ArtisanUserId = artisan.Id, CreatedByUserId = creator.Id,
            ProblemTitle = "t", ProblemDescription = "d", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });

        await Assert.ThrowsAnyAsync<Exception>(() => context.SaveChangesAsync(Ct));

        Assert.False(await db.NewContext().UserNotifications.AnyAsync(n => n.UserId != Guid.Empty && n.Title == "New artisan support case", Ct));
    }

    [Fact]
    public async Task SaveChangesAsync_AddingAUserNotificationDirectly_DoesNotGenerateANotificationAboutItself()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var user = TestUsers.Create();
        context.Users.Add(user);
        context.UserNotifications.Add(new UserNotification { UserId = user.Id, Title = "Manual", Body = "b", Category = "Activity" });

        await context.SaveChangesAsync(Ct);

        Assert.Equal(1, await db.NewContext().UserNotifications.CountAsync(n => n.UserId == user.Id, Ct));
    }
}

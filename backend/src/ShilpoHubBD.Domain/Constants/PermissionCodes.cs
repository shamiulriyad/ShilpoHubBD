namespace ShilpoHubBD.Domain.Constants;

/// <summary>
/// Seeded permission catalogue for the Super Admin permission matrix. Grouped by the admin
/// dashboard module the permission governs.
/// </summary>
public static class PermissionCodes
{
    public static readonly IReadOnlyList<(string Code, string Name, string Module, string Description)> Catalogue = new[]
    {
        ("users.view", "View Users", "Users", "View the user directory and user profiles."),
        ("users.manage", "Manage Users", "Users", "Activate, deactivate and edit user accounts."),
        ("users.roles.manage", "Manage Roles", "Users", "Assign and remove roles from users."),
        ("users.permissions.manage", "Manage Permissions", "Users", "Grant or revoke permissions on roles."),
        ("users.verification.review", "Review Identity Verifications", "Users", "Approve or reject submitted identity verification requests."),

        ("heritage.categories.manage", "Manage Craft Categories", "Heritage", "Create, edit and remove craft categories."),
        ("heritage.villages.manage", "Manage Heritage Villages", "Heritage", "Create, edit and remove heritage villages."),
        ("heritage.districts.manage", "Manage Districts", "Heritage", "Create, edit and remove districts."),
        ("heritage.festivals.manage", "Manage Festivals", "Heritage", "Create, edit and remove festivals."),
        ("heritage.unesco.manage", "Manage UNESCO Records", "Heritage", "Create, edit and remove UNESCO heritage records."),

        ("marketplace.products.approve", "Approve Products", "Marketplace", "Approve or reject product listings."),
        ("marketplace.monitor", "Monitor Marketplace", "Marketplace", "View marketplace-wide monitoring dashboards."),
        ("marketplace.refunds.manage", "Manage Refunds", "Marketplace", "Approve or reject refund requests."),
        ("marketplace.fraud.manage", "Manage Fraud Control", "Marketplace", "Review and act on fraud-control flags."),

        ("cms.homepage.manage", "Manage Homepage", "CMS", "Edit homepage content sections."),
        ("cms.blogs.manage", "Manage Blogs", "CMS", "Create, edit and remove blog posts."),
        ("cms.news.manage", "Manage News", "CMS", "Create, edit and remove news items."),
        ("cms.events.manage", "Manage Events", "CMS", "Create, edit and remove CMS events."),
        ("cms.announcements.manage", "Manage Announcements", "CMS", "Create, edit and remove announcements."),

        ("moderation.reviews.manage", "Moderate Reviews", "AI Moderation", "Review and act on fake-review flags."),
        ("moderation.spam.manage", "Moderate Spam", "AI Moderation", "Review and act on spam-detection flags."),
        ("moderation.content.manage", "Moderate Content", "AI Moderation", "Review and act on flagged content."),
        ("moderation.images.manage", "Moderate Images", "AI Moderation", "Review and act on flagged images."),

        ("security.audit.view", "View Audit Logs", "Security", "View platform audit logs."),
        ("security.backups.manage", "Manage Backups", "Security", "Trigger and manage system backups."),
        ("security.monitoring.view", "View System Monitoring", "Security", "View system health and monitoring dashboards."),
        ("security.api.manage", "Manage API Access", "Security", "Manage API keys and API access."),
        ("security.threats.manage", "Manage Threat Detection", "Security", "Review and act on threat-detection alerts."),
    };
}

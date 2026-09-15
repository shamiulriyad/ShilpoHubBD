using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUserManagementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdentityVerificationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FrontImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    BackImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SelfieImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApplicantNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityVerificationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityVerificationRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IdentityVerificationRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Module = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Users_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "Module", "Name" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000000"), "users.view", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View the user directory and user profiles.", "Users", "View Users" },
                    { new Guid("10000000-0000-0000-0000-000000000001"), "users.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Activate, deactivate and edit user accounts.", "Users", "Manage Users" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "users.roles.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Assign and remove roles from users.", "Users", "Manage Roles" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "users.permissions.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grant or revoke permissions on roles.", "Users", "Manage Permissions" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "users.verification.review", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Approve or reject submitted identity verification requests.", "Users", "Review Identity Verifications" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "heritage.categories.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create, edit and remove craft categories.", "Heritage", "Manage Craft Categories" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "heritage.villages.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create, edit and remove heritage villages.", "Heritage", "Manage Heritage Villages" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "heritage.districts.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create, edit and remove districts.", "Heritage", "Manage Districts" },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "heritage.festivals.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create, edit and remove festivals.", "Heritage", "Manage Festivals" },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "heritage.unesco.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create, edit and remove UNESCO heritage records.", "Heritage", "Manage UNESCO Records" },
                    { new Guid("10000000-0000-0000-0000-000000000010"), "marketplace.products.approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Approve or reject product listings.", "Marketplace", "Approve Products" },
                    { new Guid("10000000-0000-0000-0000-000000000011"), "marketplace.monitor", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View marketplace-wide monitoring dashboards.", "Marketplace", "Monitor Marketplace" },
                    { new Guid("10000000-0000-0000-0000-000000000012"), "marketplace.refunds.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Approve or reject refund requests.", "Marketplace", "Manage Refunds" },
                    { new Guid("10000000-0000-0000-0000-000000000013"), "marketplace.fraud.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Review and act on fraud-control flags.", "Marketplace", "Manage Fraud Control" },
                    { new Guid("10000000-0000-0000-0000-000000000014"), "cms.homepage.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit homepage content sections.", "CMS", "Manage Homepage" },
                    { new Guid("10000000-0000-0000-0000-000000000015"), "cms.blogs.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create, edit and remove blog posts.", "CMS", "Manage Blogs" },
                    { new Guid("10000000-0000-0000-0000-000000000016"), "cms.news.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create, edit and remove news items.", "CMS", "Manage News" },
                    { new Guid("10000000-0000-0000-0000-000000000017"), "cms.events.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create, edit and remove CMS events.", "CMS", "Manage Events" },
                    { new Guid("10000000-0000-0000-0000-000000000018"), "cms.announcements.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create, edit and remove announcements.", "CMS", "Manage Announcements" },
                    { new Guid("10000000-0000-0000-0000-000000000019"), "moderation.reviews.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Review and act on fake-review flags.", "AI Moderation", "Moderate Reviews" },
                    { new Guid("10000000-0000-0000-0000-000000000020"), "moderation.spam.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Review and act on spam-detection flags.", "AI Moderation", "Moderate Spam" },
                    { new Guid("10000000-0000-0000-0000-000000000021"), "moderation.content.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Review and act on flagged content.", "AI Moderation", "Moderate Content" },
                    { new Guid("10000000-0000-0000-0000-000000000022"), "moderation.images.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Review and act on flagged images.", "AI Moderation", "Moderate Images" },
                    { new Guid("10000000-0000-0000-0000-000000000023"), "security.audit.view", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View platform audit logs.", "Security", "View Audit Logs" },
                    { new Guid("10000000-0000-0000-0000-000000000024"), "security.backups.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trigger and manage system backups.", "Security", "Manage Backups" },
                    { new Guid("10000000-0000-0000-0000-000000000025"), "security.monitoring.view", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View system health and monitoring dashboards.", "Security", "View System Monitoring" },
                    { new Guid("10000000-0000-0000-0000-000000000026"), "security.api.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manage API keys and API access.", "Security", "Manage API Access" },
                    { new Guid("10000000-0000-0000-0000-000000000027"), "security.threats.manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Review and act on threat-detection alerts.", "Security", "Manage Threat Detection" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerificationRequests_ReviewedByUserId",
                table: "IdentityVerificationRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerificationRequests_Status",
                table: "IdentityVerificationRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerificationRequests_SubmittedAt",
                table: "IdentityVerificationRequests",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerificationRequests_UserId",
                table: "IdentityVerificationRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_GrantedByUserId",
                table: "RolePermissions",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentityVerificationRequests");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Permissions");
        }
    }
}

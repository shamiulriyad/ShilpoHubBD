using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResearchWorkspaceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResearchProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Discipline = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Institution = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchProjects_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResearchActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchActivities_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResearchActivities_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResearchMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AchievedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchMilestones_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResearchNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchNotes_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResearchNotes_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResearchPapers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Abstract = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Authors = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Keywords = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ManuscriptUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    TargetVenue = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchPapers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchPapers_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResearchPapers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResearchProjectMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchProjectMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchProjectMembers_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResearchProjectMembers_Users_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResearchProjectMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResearchTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchTasks_ResearchMilestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "ResearchMilestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ResearchTasks_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResearchTasks_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResearchTasks_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResearchPublications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchPaperId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Authors = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Venue = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Doi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Abstract = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Citation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PublishedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchPublications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchPublications_ResearchPapers_ResearchPaperId",
                        column: x => x.ResearchPaperId,
                        principalTable: "ResearchPapers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ResearchPublications_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResearchPublications_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchActivities_ActorUserId",
                table: "ResearchActivities",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchActivities_ResearchProjectId_CreatedAt",
                table: "ResearchActivities",
                columns: new[] { "ResearchProjectId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchMilestones_ResearchProjectId_OrderIndex",
                table: "ResearchMilestones",
                columns: new[] { "ResearchProjectId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchMilestones_Status",
                table: "ResearchMilestones",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchNotes_AuthorUserId",
                table: "ResearchNotes",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchNotes_ResearchProjectId",
                table: "ResearchNotes",
                column: "ResearchProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchPapers_CreatedByUserId",
                table: "ResearchPapers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchPapers_ResearchProjectId",
                table: "ResearchPapers",
                column: "ResearchProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchPapers_Status",
                table: "ResearchPapers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchProjectMembers_InvitedByUserId",
                table: "ResearchProjectMembers",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchProjectMembers_ResearchProjectId_UserId",
                table: "ResearchProjectMembers",
                columns: new[] { "ResearchProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResearchProjectMembers_UserId",
                table: "ResearchProjectMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchProjects_OwnerUserId",
                table: "ResearchProjects",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchProjects_Slug",
                table: "ResearchProjects",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResearchProjects_Status",
                table: "ResearchProjects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchProjects_Visibility",
                table: "ResearchProjects",
                column: "Visibility");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchPublications_CreatedByUserId",
                table: "ResearchPublications",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchPublications_IsPublic",
                table: "ResearchPublications",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchPublications_PublishedOn",
                table: "ResearchPublications",
                column: "PublishedOn");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchPublications_ResearchPaperId",
                table: "ResearchPublications",
                column: "ResearchPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchPublications_ResearchProjectId",
                table: "ResearchPublications",
                column: "ResearchProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchPublications_Type",
                table: "ResearchPublications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchTasks_AssignedToUserId",
                table: "ResearchTasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchTasks_CreatedByUserId",
                table: "ResearchTasks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchTasks_MilestoneId",
                table: "ResearchTasks",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchTasks_ResearchProjectId",
                table: "ResearchTasks",
                column: "ResearchProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchTasks_Status",
                table: "ResearchTasks",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResearchActivities");

            migrationBuilder.DropTable(
                name: "ResearchNotes");

            migrationBuilder.DropTable(
                name: "ResearchProjectMembers");

            migrationBuilder.DropTable(
                name: "ResearchPublications");

            migrationBuilder.DropTable(
                name: "ResearchTasks");

            migrationBuilder.DropTable(
                name: "ResearchPapers");

            migrationBuilder.DropTable(
                name: "ResearchMilestones");

            migrationBuilder.DropTable(
                name: "ResearchProjects");
        }
    }
}

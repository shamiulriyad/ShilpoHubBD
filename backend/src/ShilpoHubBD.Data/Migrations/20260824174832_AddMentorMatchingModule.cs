using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMentorMatchingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailabilityNote",
                table: "MentorProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "MentorProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredCategory",
                table: "MentorProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MentorshipRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MentorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LearnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageSkillId = table.Column<Guid>(type: "uuid", nullable: true),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorshipRequests_HeritageSkills_HeritageSkillId",
                        column: x => x.HeritageSkillId,
                        principalTable: "HeritageSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentorshipRequests_MentorProfiles_MentorProfileId",
                        column: x => x.MentorProfileId,
                        principalTable: "MentorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorshipRequests_Users_LearnerUserId",
                        column: x => x.LearnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MentorSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MentorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageSkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorSkills_HeritageSkills_HeritageSkillId",
                        column: x => x.HeritageSkillId,
                        principalTable: "HeritageSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentorSkills_MentorProfiles_MentorProfileId",
                        column: x => x.MentorProfileId,
                        principalTable: "MentorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipRequests_HeritageSkillId",
                table: "MentorshipRequests",
                column: "HeritageSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipRequests_LearnerUserId_Status",
                table: "MentorshipRequests",
                columns: new[] { "LearnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipRequests_MentorProfileId_Status",
                table: "MentorshipRequests",
                columns: new[] { "MentorProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MentorSkills_HeritageSkillId",
                table: "MentorSkills",
                column: "HeritageSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorSkills_MentorProfileId_HeritageSkillId",
                table: "MentorSkills",
                columns: new[] { "MentorProfileId", "HeritageSkillId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MentorshipRequests");

            migrationBuilder.DropTable(
                name: "MentorSkills");

            migrationBuilder.DropColumn(
                name: "AvailabilityNote",
                table: "MentorProfiles");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "MentorProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredCategory",
                table: "MentorProfiles");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademyProfileModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcademyMemberProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LearningPreferences = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademyMemberProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademyMemberProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HeritageSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageSkills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcademyMemberSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademyMemberProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageSkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademyMemberSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademyMemberSkills_AcademyMemberProfiles_AcademyMemberProf~",
                        column: x => x.AcademyMemberProfileId,
                        principalTable: "AcademyMemberProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AcademyMemberSkills_HeritageSkills_HeritageSkillId",
                        column: x => x.HeritageSkillId,
                        principalTable: "HeritageSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademyMemberProfiles_UserId",
                table: "AcademyMemberProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademyMemberSkills_AcademyMemberProfileId_HeritageSkillId",
                table: "AcademyMemberSkills",
                columns: new[] { "AcademyMemberProfileId", "HeritageSkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademyMemberSkills_HeritageSkillId",
                table: "AcademyMemberSkills",
                column: "HeritageSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageSkills_Name",
                table: "HeritageSkills",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcademyMemberSkills");

            migrationBuilder.DropTable(
                name: "AcademyMemberProfiles");

            migrationBuilder.DropTable(
                name: "HeritageSkills");
        }
    }
}

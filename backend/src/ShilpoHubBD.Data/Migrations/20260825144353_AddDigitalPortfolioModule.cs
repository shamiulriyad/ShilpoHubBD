using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDigitalPortfolioModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MentorFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MentorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LearnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageSkillId = table.Column<Guid>(type: "uuid", nullable: true),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorFeedbacks_HeritageSkills_HeritageSkillId",
                        column: x => x.HeritageSkillId,
                        principalTable: "HeritageSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentorFeedbacks_MentorProfiles_MentorProfileId",
                        column: x => x.MentorProfileId,
                        principalTable: "MentorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorFeedbacks_Users_LearnerUserId",
                        column: x => x.LearnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademyMemberProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Headline = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Portfolios_AcademyMemberProfiles_AcademyMemberProfileId",
                        column: x => x.AcademyMemberProfileId,
                        principalTable: "AcademyMemberProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageSkillId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProjectUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioProjects_HeritageSkills_HeritageSkillId",
                        column: x => x.HeritageSkillId,
                        principalTable: "HeritageSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PortfolioProjects_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MentorFeedbacks_HeritageSkillId",
                table: "MentorFeedbacks",
                column: "HeritageSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorFeedbacks_LearnerUserId",
                table: "MentorFeedbacks",
                column: "LearnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorFeedbacks_MentorProfileId",
                table: "MentorFeedbacks",
                column: "MentorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_HeritageSkillId",
                table: "PortfolioProjects",
                column: "HeritageSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioProjects_PortfolioId",
                table: "PortfolioProjects",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_AcademyMemberProfileId",
                table: "Portfolios",
                column: "AcademyMemberProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MentorFeedbacks");

            migrationBuilder.DropTable(
                name: "PortfolioProjects");

            migrationBuilder.DropTable(
                name: "Portfolios");
        }
    }
}

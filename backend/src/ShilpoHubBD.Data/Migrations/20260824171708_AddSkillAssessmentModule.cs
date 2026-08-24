using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillAssessmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SkillAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademyMemberProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageSkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Score = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillAssessments_AcademyMemberProfiles_AcademyMemberProfile~",
                        column: x => x.AcademyMemberProfileId,
                        principalTable: "AcademyMemberProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillAssessments_HeritageSkills_HeritageSkillId",
                        column: x => x.HeritageSkillId,
                        principalTable: "HeritageSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SkillAssessmentInsights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillAssessmentInsights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillAssessmentInsights_SkillAssessments_SkillAssessmentId",
                        column: x => x.SkillAssessmentId,
                        principalTable: "SkillAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillAssessmentRecommendedSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageSkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillAssessmentRecommendedSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillAssessmentRecommendedSkills_HeritageSkills_HeritageSki~",
                        column: x => x.HeritageSkillId,
                        principalTable: "HeritageSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillAssessmentRecommendedSkills_SkillAssessments_SkillAsse~",
                        column: x => x.SkillAssessmentId,
                        principalTable: "SkillAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkillAssessmentInsights_SkillAssessmentId_Type_DisplayOrder",
                table: "SkillAssessmentInsights",
                columns: new[] { "SkillAssessmentId", "Type", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SkillAssessmentRecommendedSkills_HeritageSkillId",
                table: "SkillAssessmentRecommendedSkills",
                column: "HeritageSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillAssessmentRecommendedSkills_SkillAssessmentId",
                table: "SkillAssessmentRecommendedSkills",
                column: "SkillAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillAssessments_AcademyMemberProfileId",
                table: "SkillAssessments",
                column: "AcademyMemberProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillAssessments_HeritageSkillId",
                table: "SkillAssessments",
                column: "HeritageSkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkillAssessmentInsights");

            migrationBuilder.DropTable(
                name: "SkillAssessmentRecommendedSkills");

            migrationBuilder.DropTable(
                name: "SkillAssessments");
        }
    }
}

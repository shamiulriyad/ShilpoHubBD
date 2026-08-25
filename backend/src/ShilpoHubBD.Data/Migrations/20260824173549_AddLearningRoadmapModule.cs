using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningRoadmapModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningRoadmaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademyMemberProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Goal = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TargetHeritageSkillId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningRoadmaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningRoadmaps_AcademyMemberProfiles_AcademyMemberProfile~",
                        column: x => x.AcademyMemberProfileId,
                        principalTable: "AcademyMemberProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningRoadmaps_HeritageSkills_TargetHeritageSkillId",
                        column: x => x.TargetHeritageSkillId,
                        principalTable: "HeritageSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LearningRoadmapId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageSkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapMilestones_HeritageSkills_HeritageSkillId",
                        column: x => x.HeritageSkillId,
                        principalTable: "HeritageSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoadmapMilestones_LearningRoadmaps_LearningRoadmapId",
                        column: x => x.LearningRoadmapId,
                        principalTable: "LearningRoadmaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapRecommendedCourses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoadmapMilestoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapRecommendedCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapRecommendedCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoadmapRecommendedCourses_RoadmapMilestones_RoadmapMileston~",
                        column: x => x.RoadmapMilestoneId,
                        principalTable: "RoadmapMilestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapRecommendedLessons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoadmapMilestoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseLessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapRecommendedLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapRecommendedLessons_CourseLessons_CourseLessonId",
                        column: x => x.CourseLessonId,
                        principalTable: "CourseLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoadmapRecommendedLessons_RoadmapMilestones_RoadmapMileston~",
                        column: x => x.RoadmapMilestoneId,
                        principalTable: "RoadmapMilestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningRoadmaps_AcademyMemberProfileId_Status",
                table: "LearningRoadmaps",
                columns: new[] { "AcademyMemberProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LearningRoadmaps_TargetHeritageSkillId",
                table: "LearningRoadmaps",
                column: "TargetHeritageSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapMilestones_HeritageSkillId",
                table: "RoadmapMilestones",
                column: "HeritageSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapMilestones_LearningRoadmapId_DisplayOrder",
                table: "RoadmapMilestones",
                columns: new[] { "LearningRoadmapId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapRecommendedCourses_CourseId",
                table: "RoadmapRecommendedCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapRecommendedCourses_RoadmapMilestoneId",
                table: "RoadmapRecommendedCourses",
                column: "RoadmapMilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapRecommendedLessons_CourseLessonId",
                table: "RoadmapRecommendedLessons",
                column: "CourseLessonId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapRecommendedLessons_RoadmapMilestoneId",
                table: "RoadmapRecommendedLessons",
                column: "RoadmapMilestoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadmapRecommendedCourses");

            migrationBuilder.DropTable(
                name: "RoadmapRecommendedLessons");

            migrationBuilder.DropTable(
                name: "RoadmapMilestones");

            migrationBuilder.DropTable(
                name: "LearningRoadmaps");
        }
    }
}

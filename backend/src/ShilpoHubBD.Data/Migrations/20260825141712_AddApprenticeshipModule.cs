using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApprenticeshipModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprenticeshipPrograms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MentorId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainerProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    HeritageSkillId = table.Column<Guid>(type: "uuid", nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DurationWeeks = table.Column<int>(type: "integer", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    EligibilityRequirements = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprenticeshipPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprenticeshipPrograms_AcademyMemberProfiles_TrainerProfile~",
                        column: x => x.TrainerProfileId,
                        principalTable: "AcademyMemberProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApprenticeshipPrograms_HeritageSkills_HeritageSkillId",
                        column: x => x.HeritageSkillId,
                        principalTable: "HeritageSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApprenticeshipPrograms_MentorProfiles_MentorId",
                        column: x => x.MentorId,
                        principalTable: "MentorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramApplications_ApprenticeshipPrograms_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "ApprenticeshipPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramApplications_Users_ApplicantUserId",
                        column: x => x.ApplicantUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingMilestones_ApprenticeshipPrograms_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "ApprenticeshipPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApprenticeEnrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprenticeUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprenticeEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprenticeEnrollments_ApprenticeshipPrograms_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "ApprenticeshipPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApprenticeEnrollments_ProgramApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ProgramApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApprenticeEnrollments_Users_ApprenticeUserId",
                        column: x => x.ApprenticeUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApprenticeMilestoneProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprenticeMilestoneProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprenticeMilestoneProgress_ApprenticeEnrollments_Enrollmen~",
                        column: x => x.EnrollmentId,
                        principalTable: "ApprenticeEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprenticeMilestoneProgress_TrainingMilestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "TrainingMilestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprenticeEnrollments_ApplicationId",
                table: "ApprenticeEnrollments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprenticeEnrollments_ApprenticeUserId",
                table: "ApprenticeEnrollments",
                column: "ApprenticeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprenticeEnrollments_ProgramId_ApprenticeUserId",
                table: "ApprenticeEnrollments",
                columns: new[] { "ProgramId", "ApprenticeUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprenticeMilestoneProgress_EnrollmentId_MilestoneId",
                table: "ApprenticeMilestoneProgress",
                columns: new[] { "EnrollmentId", "MilestoneId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprenticeMilestoneProgress_MilestoneId",
                table: "ApprenticeMilestoneProgress",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprenticeshipPrograms_HeritageSkillId",
                table: "ApprenticeshipPrograms",
                column: "HeritageSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprenticeshipPrograms_MentorId",
                table: "ApprenticeshipPrograms",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprenticeshipPrograms_Status",
                table: "ApprenticeshipPrograms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ApprenticeshipPrograms_TrainerProfileId",
                table: "ApprenticeshipPrograms",
                column: "TrainerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprenticeshipPrograms_Type",
                table: "ApprenticeshipPrograms",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramApplications_ApplicantUserId_Status",
                table: "ProgramApplications",
                columns: new[] { "ApplicantUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramApplications_ProgramId_Status",
                table: "ProgramApplications",
                columns: new[] { "ProgramId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingMilestones_ProgramId",
                table: "TrainingMilestones",
                column: "ProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprenticeMilestoneProgress");

            migrationBuilder.DropTable(
                name: "ApprenticeEnrollments");

            migrationBuilder.DropTable(
                name: "TrainingMilestones");

            migrationBuilder.DropTable(
                name: "ProgramApplications");

            migrationBuilder.DropTable(
                name: "ApprenticeshipPrograms");
        }
    }
}

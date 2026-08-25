using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalIntegrationIndexFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProgramApplications_ProgramId_ApplicantUserId",
                table: "ProgramApplications",
                columns: new[] { "ProgramId", "ApplicantUserId" },
                unique: true,
                filter: "\"Status\" IN ('Pending', 'Accepted')");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobListingId_ApplicantUserId",
                table: "JobApplications",
                columns: new[] { "JobListingId", "ApplicantUserId" },
                unique: true,
                filter: "\"Status\" IN ('Pending', 'Shortlisted')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProgramApplications_ProgramId_ApplicantUserId",
                table: "ProgramApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_JobListingId_ApplicantUserId",
                table: "JobApplications");
        }
    }
}

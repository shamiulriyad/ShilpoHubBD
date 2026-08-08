using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeritageLegacyScoreHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeritageScoreHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerHeritageIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    YearsOfExperiencePoints = table.Column<int>(type: "integer", nullable: false),
                    VerificationPoints = table.Column<int>(type: "integer", nullable: false),
                    AwardsPoints = table.Column<int>(type: "integer", nullable: false),
                    CertificationsPoints = table.Column<int>(type: "integer", nullable: false),
                    ProductsPoints = table.Column<int>(type: "integer", nullable: false),
                    ReviewsPoints = table.Column<int>(type: "integer", nullable: false),
                    ApprenticesTrainedPoints = table.Column<int>(type: "integer", nullable: false),
                    CoursesPublishedPoints = table.Column<int>(type: "integer", nullable: false),
                    CulturalContributionPoints = table.Column<int>(type: "integer", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageScoreHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageScoreHistory_ProducerHeritageIdentities_ProducerHer~",
                        column: x => x.ProducerHeritageIdentityId,
                        principalTable: "ProducerHeritageIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HeritageScoreHistory_ProducerHeritageIdentityId_CalculatedAt",
                table: "HeritageScoreHistory",
                columns: new[] { "ProducerHeritageIdentityId", "CalculatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeritageScoreHistory");
        }
    }
}

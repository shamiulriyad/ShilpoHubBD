using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerImpactAIAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProducerImpactAIAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImpactAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsAiGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerImpactAIAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerImpactAIAnalyses_ArtisanSupportCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ArtisanSupportCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProducerImpactAIAnalyses_ArtisanSupportImpactAssessments_Im~",
                        column: x => x.ImpactAssessmentId,
                        principalTable: "ArtisanSupportImpactAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProducerImpactAIAnalyses_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProducerImpactAIFindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerImpactAIFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerImpactAIFindings_ProducerImpactAIAnalyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "ProducerImpactAIAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProducerImpactAIAnalyses_CaseId",
                table: "ProducerImpactAIAnalyses",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerImpactAIAnalyses_ImpactAssessmentId",
                table: "ProducerImpactAIAnalyses",
                column: "ImpactAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerImpactAIAnalyses_RequestedByUserId",
                table: "ProducerImpactAIAnalyses",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerImpactAIFindings_AnalysisId",
                table: "ProducerImpactAIFindings",
                column: "AnalysisId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProducerImpactAIFindings");

            migrationBuilder.DropTable(
                name: "ProducerImpactAIAnalyses");
        }
    }
}

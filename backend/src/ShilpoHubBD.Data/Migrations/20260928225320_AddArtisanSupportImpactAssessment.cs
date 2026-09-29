using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArtisanSupportImpactAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArtisanSupportImpactAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeforeYear = table.Column<int>(type: "integer", nullable: false),
                    BeforeMonth = table.Column<int>(type: "integer", nullable: false),
                    BeforeReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    AfterYear = table.Column<int>(type: "integer", nullable: false),
                    AfterMonth = table.Column<int>(type: "integer", nullable: false),
                    AfterReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtisanSupportImpactAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportImpactAssessments_ArtisanSupportCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ArtisanSupportCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportImpactAssessments_ProducerMonthlyReports_Afte~",
                        column: x => x.AfterReportId,
                        principalTable: "ProducerMonthlyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportImpactAssessments_ProducerMonthlyReports_Befo~",
                        column: x => x.BeforeReportId,
                        principalTable: "ProducerMonthlyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportImpactAssessments_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArtisanSupportImpactMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BeforeValue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AfterValue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ChangeAbsolute = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ChangePercentage = table.Column<decimal>(type: "numeric(9,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtisanSupportImpactMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportImpactMetrics_ArtisanSupportImpactAssessments~",
                        column: x => x.AssessmentId,
                        principalTable: "ArtisanSupportImpactAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportImpactAssessments_AfterReportId",
                table: "ArtisanSupportImpactAssessments",
                column: "AfterReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportImpactAssessments_BeforeReportId",
                table: "ArtisanSupportImpactAssessments",
                column: "BeforeReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportImpactAssessments_CaseId",
                table: "ArtisanSupportImpactAssessments",
                column: "CaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportImpactAssessments_GeneratedByUserId",
                table: "ArtisanSupportImpactAssessments",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportImpactMetrics_AssessmentId_MetricType",
                table: "ArtisanSupportImpactMetrics",
                columns: new[] { "AssessmentId", "MetricType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtisanSupportImpactMetrics");

            migrationBuilder.DropTable(
                name: "ArtisanSupportImpactAssessments");
        }
    }
}

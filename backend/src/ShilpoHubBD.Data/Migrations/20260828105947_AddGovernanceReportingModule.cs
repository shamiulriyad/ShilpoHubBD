using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGovernanceReportingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GovForecasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Method = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    HorizonMonths = table.Column<int>(type: "integer", nullable: false),
                    BaselineAsOf = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssumptionsJson = table.Column<string>(type: "text", nullable: true),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovForecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovForecasts_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GovReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReportType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Highlights = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovReports_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GovForecastPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GovForecastId = table.Column<Guid>(type: "uuid", nullable: false),
                    Metric = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MonthOffset = table.Column<int>(type: "integer", nullable: false),
                    PeriodDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BaselineValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProjectedValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LowerBound = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UpperBound = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Confidence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovForecastPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovForecastPoints_GovForecasts_GovForecastId",
                        column: x => x.GovForecastId,
                        principalTable: "GovForecasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsExports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Dataset = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Format = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FiltersJson = table.Column<string>(type: "text", nullable: true),
                    RowCount = table.Column<int>(type: "integer", nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GovReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsExports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsExports_GovReports_GovReportId",
                        column: x => x.GovReportId,
                        principalTable: "GovReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AnalyticsExports_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GovReportSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GovReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Narrative = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ContentJson = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovReportSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovReportSections_GovReports_GovReportId",
                        column: x => x.GovReportId,
                        principalTable: "GovReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_Dataset",
                table: "AnalyticsExports",
                column: "Dataset");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_GovReportId",
                table: "AnalyticsExports",
                column: "GovReportId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_RequestedAt",
                table: "AnalyticsExports",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_RequestedByUserId",
                table: "AnalyticsExports",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsExports_Status",
                table: "AnalyticsExports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GovForecastPoints_GovForecastId_DisplayOrder",
                table: "GovForecastPoints",
                columns: new[] { "GovForecastId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_GovForecasts_GeneratedAt",
                table: "GovForecasts",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GovForecasts_GeneratedByUserId",
                table: "GovForecasts",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GovReports_GeneratedByUserId",
                table: "GovReports",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GovReports_PeriodEnd",
                table: "GovReports",
                column: "PeriodEnd");

            migrationBuilder.CreateIndex(
                name: "IX_GovReports_ReportType",
                table: "GovReports",
                column: "ReportType");

            migrationBuilder.CreateIndex(
                name: "IX_GovReports_Status",
                table: "GovReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GovReportSections_GovReportId_DisplayOrder",
                table: "GovReportSections",
                columns: new[] { "GovReportId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsExports");

            migrationBuilder.DropTable(
                name: "GovForecastPoints");

            migrationBuilder.DropTable(
                name: "GovReportSections");

            migrationBuilder.DropTable(
                name: "GovForecasts");

            migrationBuilder.DropTable(
                name: "GovReports");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGovernanceNationalDashboardModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NationalDashboardSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalProducers = table.Column<int>(type: "integer", nullable: false),
                    ActiveProducers = table.Column<int>(type: "integer", nullable: false),
                    VerifiedHeritageProducers = table.Column<int>(type: "integer", nullable: false),
                    NewProducers = table.Column<int>(type: "integer", nullable: false),
                    JobsPosted = table.Column<int>(type: "integer", nullable: false),
                    JobApplications = table.Column<int>(type: "integer", nullable: false),
                    JobsFilled = table.Column<int>(type: "integer", nullable: false),
                    ExporterPartners = table.Column<int>(type: "integer", nullable: false),
                    ExportOrders = table.Column<int>(type: "integer", nullable: false),
                    ExportSalesValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    ProductsSold = table.Column<int>(type: "integer", nullable: false),
                    MarketplaceSalesValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HeritageEconomyValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TourismBookings = table.Column<int>(type: "integer", nullable: false),
                    TourismRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TouristsServed = table.Column<int>(type: "integer", nullable: false),
                    DistrictsCovered = table.Column<int>(type: "integer", nullable: false),
                    VillagesCovered = table.Column<int>(type: "integer", nullable: false),
                    ProductsListed = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NationalDashboardSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NationalDashboardSnapshots_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DashboardDistrictStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NationalDashboardSnapshotId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Division = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ProducerCount = table.Column<int>(type: "integer", nullable: false),
                    ProductCount = table.Column<int>(type: "integer", nullable: false),
                    VillageCount = table.Column<int>(type: "integer", nullable: false),
                    OrderCount = table.Column<int>(type: "integer", nullable: false),
                    SalesValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardDistrictStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardDistrictStats_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DashboardDistrictStats_NationalDashboardSnapshots_NationalD~",
                        column: x => x.NationalDashboardSnapshotId,
                        principalTable: "NationalDashboardSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardDistrictStats_DistrictId",
                table: "DashboardDistrictStats",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardDistrictStats_NationalDashboardSnapshotId_Rank",
                table: "DashboardDistrictStats",
                columns: new[] { "NationalDashboardSnapshotId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_NationalDashboardSnapshots_GeneratedByUserId",
                table: "NationalDashboardSnapshots",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NationalDashboardSnapshots_Period",
                table: "NationalDashboardSnapshots",
                column: "Period");

            migrationBuilder.CreateIndex(
                name: "IX_NationalDashboardSnapshots_PeriodEnd",
                table: "NationalDashboardSnapshots",
                column: "PeriodEnd");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardDistrictStats");

            migrationBuilder.DropTable(
                name: "NationalDashboardSnapshots");
        }
    }
}

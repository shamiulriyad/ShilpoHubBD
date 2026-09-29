using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerMonthlyReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProducerMonthlyReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    TotalSales = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetIncome = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    CancelledOrders = table.Column<int>(type: "integer", nullable: false),
                    CancellationRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ProductCount = table.Column<int>(type: "integer", nullable: false),
                    UnitsSold = table.Column<int>(type: "integer", nullable: false),
                    NewCustomers = table.Column<int>(type: "integer", nullable: false),
                    ReturningCustomers = table.Column<int>(type: "integer", nullable: false),
                    PreviousMonthSales = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SalesGrowthPercentage = table.Column<decimal>(type: "numeric(9,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerMonthlyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerMonthlyReports_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProducerMonthlyReports_ProducerId_Year_Month",
                table: "ProducerMonthlyReports",
                columns: new[] { "ProducerId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProducerMonthlyReports_Year_Month",
                table: "ProducerMonthlyReports",
                columns: new[] { "Year", "Month" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProducerMonthlyReports");
        }
    }
}

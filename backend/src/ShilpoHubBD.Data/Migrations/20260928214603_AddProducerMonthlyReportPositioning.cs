using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerMonthlyReportPositioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CategoryAverageSales",
                table: "ProducerMonthlyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "ProducerMonthlyReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryPosition",
                table: "ProducerMonthlyReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DistrictAverageSales",
                table: "ProducerMonthlyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                table: "ProducerMonthlyReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DistrictPosition",
                table: "ProducerMonthlyReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OverallSalesPercentile",
                table: "ProducerMonthlyReports",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OverallSalesRank",
                table: "ProducerMonthlyReports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PeerAverageGrowthPercentage",
                table: "ProducerMonthlyReports",
                type: "numeric(9,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProducerMonthlyReports_CategoryId",
                table: "ProducerMonthlyReports",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerMonthlyReports_DistrictId",
                table: "ProducerMonthlyReports",
                column: "DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProducerMonthlyReports_Categories_CategoryId",
                table: "ProducerMonthlyReports",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProducerMonthlyReports_Districts_DistrictId",
                table: "ProducerMonthlyReports",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProducerMonthlyReports_Categories_CategoryId",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropForeignKey(
                name: "FK_ProducerMonthlyReports_Districts_DistrictId",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropIndex(
                name: "IX_ProducerMonthlyReports_CategoryId",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropIndex(
                name: "IX_ProducerMonthlyReports_DistrictId",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropColumn(
                name: "CategoryAverageSales",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropColumn(
                name: "CategoryPosition",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropColumn(
                name: "DistrictAverageSales",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropColumn(
                name: "DistrictPosition",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropColumn(
                name: "OverallSalesPercentile",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropColumn(
                name: "OverallSalesRank",
                table: "ProducerMonthlyReports");

            migrationBuilder.DropColumn(
                name: "PeerAverageGrowthPercentage",
                table: "ProducerMonthlyReports");
        }
    }
}

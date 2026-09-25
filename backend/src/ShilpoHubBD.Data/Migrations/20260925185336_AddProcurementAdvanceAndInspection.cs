using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementAdvanceAndInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdvanceAmount",
                table: "ProcurementRequests",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdvanceMethod",
                table: "ProcurementRequests",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AdvancePaidAt",
                table: "ProcurementRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdvanceReference",
                table: "ProcurementRequests",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AdvanceRefundedAt",
                table: "ProcurementRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InspectedAt",
                table: "ProcurementRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InspectedByUserId",
                table: "ProcurementRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InspectionNotes",
                table: "ProcurementRequests",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InspectionStatus",
                table: "ProcurementRequests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "NotRequired");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_InspectedByUserId",
                table: "ProcurementRequests",
                column: "InspectedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcurementRequests_Users_InspectedByUserId",
                table: "ProcurementRequests",
                column: "InspectedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcurementRequests_Users_InspectedByUserId",
                table: "ProcurementRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProcurementRequests_InspectedByUserId",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "AdvanceAmount",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "AdvanceMethod",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "AdvancePaidAt",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "AdvanceReference",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "AdvanceRefundedAt",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "InspectedAt",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "InspectedByUserId",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "InspectionNotes",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "InspectionStatus",
                table: "ProcurementRequests");
        }
    }
}

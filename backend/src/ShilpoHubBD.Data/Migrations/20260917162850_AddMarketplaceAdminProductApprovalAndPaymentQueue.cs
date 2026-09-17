using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketplaceAdminProductApprovalAndPaymentQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing products are already live on the storefront — grandfather them in as Approved.
            // New inserts always pass an explicit ApprovalStatus (Product.cs defaults new rows to Pending).
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Products",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Approved");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ApprovalStatus_IsActive",
                table: "Products",
                columns: new[] { "ApprovalStatus", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ApprovedByUserId",
                table: "Products",
                column: "ApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_ApprovedByUserId",
                table: "Products",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_ApprovedByUserId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ApprovalStatus_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ApprovedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Products");
        }
    }
}

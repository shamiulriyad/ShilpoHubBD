using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomOrderLogisticsHandoff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Carrier",
                table: "CustomOrderRequests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "CustomOrderRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "CustomOrderRequests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientPhone",
                table: "CustomOrderRequests",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedAt",
                table: "CustomOrderRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddressLine",
                table: "CustomOrderRequests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShippingDistrictId",
                table: "CustomOrderRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackingNumber",
                table: "CustomOrderRequests",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomOrderRequests_TrackingNumber",
                table: "CustomOrderRequests",
                column: "TrackingNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomOrderRequests_TrackingNumber",
                table: "CustomOrderRequests");

            migrationBuilder.DropColumn(
                name: "Carrier",
                table: "CustomOrderRequests");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "CustomOrderRequests");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "CustomOrderRequests");

            migrationBuilder.DropColumn(
                name: "RecipientPhone",
                table: "CustomOrderRequests");

            migrationBuilder.DropColumn(
                name: "ShippedAt",
                table: "CustomOrderRequests");

            migrationBuilder.DropColumn(
                name: "ShippingAddressLine",
                table: "CustomOrderRequests");

            migrationBuilder.DropColumn(
                name: "ShippingDistrictId",
                table: "CustomOrderRequests");

            migrationBuilder.DropColumn(
                name: "TrackingNumber",
                table: "CustomOrderRequests");
        }
    }
}

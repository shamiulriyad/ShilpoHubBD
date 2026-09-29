using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminManagedLogisticsPartners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LogisticsServiceAreas_LogisticsPartnerProfileId_DistrictId",
                table: "LogisticsServiceAreas");

            migrationBuilder.AddColumn<decimal>(
                name: "PartnerRevenue",
                table: "Shipments",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickupRequestedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnReason",
                table: "Shipments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShilpoHubRevenue",
                table: "Shipments",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryCharge",
                table: "Orders",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryMethod",
                table: "Orders",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryPartnerName",
                table: "Orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedDeliveryAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LogisticsPartnerProfileId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LogisticsPartnerRevenue",
                table: "Orders",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShilpoHubDeliveryRevenue",
                table: "Orders",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShippingArea",
                table: "Orders",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AreaName",
                table: "LogisticsServiceAreas",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryCharge",
                table: "LogisticsServiceAreas",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryMethod",
                table: "LogisticsServiceAreas",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Standard");

            migrationBuilder.AddColumn<bool>(
                name: "PickupAvailable",
                table: "LogisticsServiceAreas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReturnSupported",
                table: "LogisticsServiceAreas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "LogisticsPartnerProfiles",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LogisticsPartnerProfiles",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "LogisticsPartnerProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "LogisticsPartnerProfiles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OffersPickup",
                table: "LogisticsPartnerProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsReturns",
                table: "LogisticsPartnerProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_LogisticsPartnerProfileId",
                table: "Orders",
                column: "LogisticsPartnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsServiceAreas_LogisticsPartnerProfileId_DistrictId_~",
                table: "LogisticsServiceAreas",
                columns: new[] { "LogisticsPartnerProfileId", "DistrictId", "AreaName", "DeliveryMethod" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_LogisticsPartnerProfiles_LogisticsPartnerProfileId",
                table: "Orders",
                column: "LogisticsPartnerProfileId",
                principalTable: "LogisticsPartnerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_LogisticsPartnerProfiles_LogisticsPartnerProfileId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_LogisticsPartnerProfileId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_LogisticsServiceAreas_LogisticsPartnerProfileId_DistrictId_~",
                table: "LogisticsServiceAreas");

            migrationBuilder.DropColumn(
                name: "PartnerRevenue",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PickedUpAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PickupRequestedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReturnReason",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShilpoHubRevenue",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryCharge",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryPartnerName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExpectedDeliveryAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LogisticsPartnerProfileId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LogisticsPartnerRevenue",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShilpoHubDeliveryRevenue",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingArea",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AreaName",
                table: "LogisticsServiceAreas");

            migrationBuilder.DropColumn(
                name: "DeliveryCharge",
                table: "LogisticsServiceAreas");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                table: "LogisticsServiceAreas");

            migrationBuilder.DropColumn(
                name: "PickupAvailable",
                table: "LogisticsServiceAreas");

            migrationBuilder.DropColumn(
                name: "ReturnSupported",
                table: "LogisticsServiceAreas");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "LogisticsPartnerProfiles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "LogisticsPartnerProfiles");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "LogisticsPartnerProfiles");

            migrationBuilder.DropColumn(
                name: "OffersPickup",
                table: "LogisticsPartnerProfiles");

            migrationBuilder.DropColumn(
                name: "SupportsReturns",
                table: "LogisticsPartnerProfiles");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "LogisticsPartnerProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsServiceAreas_LogisticsPartnerProfileId_DistrictId",
                table: "LogisticsServiceAreas",
                columns: new[] { "LogisticsPartnerProfileId", "DistrictId" },
                unique: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveEventAuctionLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Carrier",
                table: "OrderItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProducerNote",
                table: "OrderItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProducerRespondedAt",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProducerStatus",
                table: "OrderItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedAt",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackingNumber",
                table: "OrderItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AuctionId",
                table: "LiveEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProducerStatus",
                table: "OrderItems",
                column: "ProducerStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_AuctionId",
                table: "LiveEvents",
                column: "AuctionId");

            migrationBuilder.AddForeignKey(
                name: "FK_LiveEvents_Auctions_AuctionId",
                table: "LiveEvents",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LiveEvents_Auctions_AuctionId",
                table: "LiveEvents");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProducerStatus",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_LiveEvents_AuctionId",
                table: "LiveEvents");

            migrationBuilder.DropColumn(
                name: "Carrier",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProducerNote",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProducerRespondedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProducerStatus",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ShippedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TrackingNumber",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "AuctionId",
                table: "LiveEvents");
        }
    }
}

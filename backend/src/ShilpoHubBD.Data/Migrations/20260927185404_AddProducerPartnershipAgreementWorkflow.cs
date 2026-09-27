using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerPartnershipAgreementWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TerminationReason",
                table: "ProducerPartnershipAgreements",
                newName: "EndReason");

            migrationBuilder.RenameColumn(
                name: "TerminatedAt",
                table: "ProducerPartnershipAgreements",
                newName: "ProducerConfirmedAt");

            migrationBuilder.RenameColumn(
                name: "RevenueSharePercentage",
                table: "ProducerPartnershipAgreements",
                newName: "ProducerSharePercentage");

            migrationBuilder.RenameColumn(
                name: "ProducerAcceptedAt",
                table: "ProducerPartnershipAgreements",
                newName: "EndedAt");

            migrationBuilder.AddColumn<Guid>(
                name: "AuctionLotId",
                table: "ProducerPartnershipAgreements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BusinessPartnerConfirmedAt",
                table: "ProducerPartnershipAgreements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BusinessPartnerSharePercentage",
                table: "ProducerPartnershipAgreements",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnershipDurationMonths",
                table: "ProducerPartnershipAgreements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformFeePercentage",
                table: "ProducerPartnershipAgreements",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementFrequency",
                table: "ProducerPartnershipAgreements",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WinningBidAmount",
                table: "ProducerPartnershipAgreements",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAgreements_AuctionLotId",
                table: "ProducerPartnershipAgreements",
                column: "AuctionLotId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProducerPartnershipAgreements_ProducerPartnershipAuctionLot~",
                table: "ProducerPartnershipAgreements",
                column: "AuctionLotId",
                principalTable: "ProducerPartnershipAuctionLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProducerPartnershipAgreements_ProducerPartnershipAuctionLot~",
                table: "ProducerPartnershipAgreements");

            migrationBuilder.DropIndex(
                name: "IX_ProducerPartnershipAgreements_AuctionLotId",
                table: "ProducerPartnershipAgreements");

            migrationBuilder.DropColumn(
                name: "AuctionLotId",
                table: "ProducerPartnershipAgreements");

            migrationBuilder.DropColumn(
                name: "BusinessPartnerConfirmedAt",
                table: "ProducerPartnershipAgreements");

            migrationBuilder.DropColumn(
                name: "BusinessPartnerSharePercentage",
                table: "ProducerPartnershipAgreements");

            migrationBuilder.DropColumn(
                name: "PartnershipDurationMonths",
                table: "ProducerPartnershipAgreements");

            migrationBuilder.DropColumn(
                name: "PlatformFeePercentage",
                table: "ProducerPartnershipAgreements");

            migrationBuilder.DropColumn(
                name: "SettlementFrequency",
                table: "ProducerPartnershipAgreements");

            migrationBuilder.DropColumn(
                name: "WinningBidAmount",
                table: "ProducerPartnershipAgreements");

            migrationBuilder.RenameColumn(
                name: "ProducerSharePercentage",
                table: "ProducerPartnershipAgreements",
                newName: "RevenueSharePercentage");

            migrationBuilder.RenameColumn(
                name: "ProducerConfirmedAt",
                table: "ProducerPartnershipAgreements",
                newName: "TerminatedAt");

            migrationBuilder.RenameColumn(
                name: "EndedAt",
                table: "ProducerPartnershipAgreements",
                newName: "ProducerAcceptedAt");

            migrationBuilder.RenameColumn(
                name: "EndReason",
                table: "ProducerPartnershipAgreements",
                newName: "TerminationReason");
        }
    }
}

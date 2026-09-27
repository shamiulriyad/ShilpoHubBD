using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerPartnershipAuctionLotsAndBidding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuctionDurationHours",
                table: "ProducerPartnershipAuctions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessPartnerEligibilityCriteria",
                table: "ProducerPartnershipAuctions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxProducersPerBusinessPartner",
                table: "ProducerPartnershipAuctions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumBidIncrement",
                table: "ProducerPartnershipAuctions",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumStartingBid",
                table: "ProducerPartnershipAuctions",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ProducerPartnershipAuctionParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerPartnershipAuctionParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAuctionParticipants_ProducerPartnershipA~",
                        column: x => x.AuctionId,
                        principalTable: "ProducerPartnershipAuctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAuctionParticipants_Users_BusinessPartne~",
                        column: x => x.BusinessPartnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAuctionParticipants_Users_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProducerPartnershipAuctionBids",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerPartnershipAuctionBids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAuctionBids_Users_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProducerPartnershipAuctionLots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartingBid = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrentHighestBid = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CurrentHighestBidderId = table.Column<Guid>(type: "uuid", nullable: true),
                    BidCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WinningBidId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerPartnershipAuctionLots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAuctionLots_ProducerPartnershipAuctionBi~",
                        column: x => x.WinningBidId,
                        principalTable: "ProducerPartnershipAuctionBids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAuctionLots_ProducerPartnershipAuctions_~",
                        column: x => x.AuctionId,
                        principalTable: "ProducerPartnershipAuctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAuctionLots_Users_CurrentHighestBidderId",
                        column: x => x.CurrentHighestBidderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAuctionLots_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionBids_BusinessPartnerId",
                table: "ProducerPartnershipAuctionBids",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionBids_LotId",
                table: "ProducerPartnershipAuctionBids",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionBids_LotId_Amount",
                table: "ProducerPartnershipAuctionBids",
                columns: new[] { "LotId", "Amount" });

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionLots_AuctionId_ProducerId",
                table: "ProducerPartnershipAuctionLots",
                columns: new[] { "AuctionId", "ProducerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionLots_CurrentHighestBidderId",
                table: "ProducerPartnershipAuctionLots",
                column: "CurrentHighestBidderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionLots_ProducerId",
                table: "ProducerPartnershipAuctionLots",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionLots_Status",
                table: "ProducerPartnershipAuctionLots",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionLots_WinningBidId",
                table: "ProducerPartnershipAuctionLots",
                column: "WinningBidId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionParticipants_AuctionId_BusinessPa~",
                table: "ProducerPartnershipAuctionParticipants",
                columns: new[] { "AuctionId", "BusinessPartnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionParticipants_BusinessPartnerId",
                table: "ProducerPartnershipAuctionParticipants",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionParticipants_DecidedByUserId",
                table: "ProducerPartnershipAuctionParticipants",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctionParticipants_Status",
                table: "ProducerPartnershipAuctionParticipants",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_ProducerPartnershipAuctionBids_ProducerPartnershipAuctionLo~",
                table: "ProducerPartnershipAuctionBids",
                column: "LotId",
                principalTable: "ProducerPartnershipAuctionLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProducerPartnershipAuctionBids_ProducerPartnershipAuctionLo~",
                table: "ProducerPartnershipAuctionBids");

            migrationBuilder.DropTable(
                name: "ProducerPartnershipAuctionParticipants");

            migrationBuilder.DropTable(
                name: "ProducerPartnershipAuctionLots");

            migrationBuilder.DropTable(
                name: "ProducerPartnershipAuctionBids");

            migrationBuilder.DropColumn(
                name: "AuctionDurationHours",
                table: "ProducerPartnershipAuctions");

            migrationBuilder.DropColumn(
                name: "BusinessPartnerEligibilityCriteria",
                table: "ProducerPartnershipAuctions");

            migrationBuilder.DropColumn(
                name: "MaxProducersPerBusinessPartner",
                table: "ProducerPartnershipAuctions");

            migrationBuilder.DropColumn(
                name: "MinimumBidIncrement",
                table: "ProducerPartnershipAuctions");

            migrationBuilder.DropColumn(
                name: "MinimumStartingBid",
                table: "ProducerPartnershipAuctions");
        }
    }
}

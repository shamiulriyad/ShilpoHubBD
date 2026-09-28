using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerPartnershipFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProducerPartnershipAuctions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AuctionYear = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EligibilityCriteria = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RegistrationOpensAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegistrationClosesAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BiddingOpensAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BiddingClosesAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ParticipationFee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DefaultPartnershipDurationMonths = table.Column<int>(type: "integer", nullable: false),
                    DefaultRevenueSharePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    SettlementRulesDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ManagedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerPartnershipAuctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAuctions_Users_ManagedByUserId",
                        column: x => x.ManagedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProducerPartnershipAgreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevenueSharePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    AgreementTerms = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ProducerAcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TerminatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TerminationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerPartnershipAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAgreements_ProducerPartnershipAuctions_A~",
                        column: x => x.AuctionId,
                        principalTable: "ProducerPartnershipAuctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAgreements_Users_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipAgreements_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProducerPartnershipStatusEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgreementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerPartnershipStatusEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipStatusEvents_ProducerPartnershipAgreemen~",
                        column: x => x.AgreementId,
                        principalTable: "ProducerPartnershipAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipStatusEvents_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAgreements_AuctionId",
                table: "ProducerPartnershipAgreements",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAgreements_BusinessPartnerId",
                table: "ProducerPartnershipAgreements",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAgreements_ProducerId",
                table: "ProducerPartnershipAgreements",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAgreements_Status",
                table: "ProducerPartnershipAgreements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctions_AuctionYear",
                table: "ProducerPartnershipAuctions",
                column: "AuctionYear");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctions_ManagedByUserId",
                table: "ProducerPartnershipAuctions",
                column: "ManagedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctions_Slug",
                table: "ProducerPartnershipAuctions",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipAuctions_Status",
                table: "ProducerPartnershipAuctions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipStatusEvents_AgreementId",
                table: "ProducerPartnershipStatusEvents",
                column: "AgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipStatusEvents_ChangedByUserId",
                table: "ProducerPartnershipStatusEvents",
                column: "ChangedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProducerPartnershipStatusEvents");

            migrationBuilder.DropTable(
                name: "ProducerPartnershipAgreements");

            migrationBuilder.DropTable(
                name: "ProducerPartnershipAuctions");
        }
    }
}

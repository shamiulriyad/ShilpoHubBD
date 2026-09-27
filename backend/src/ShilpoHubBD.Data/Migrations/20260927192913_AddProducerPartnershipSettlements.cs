using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerPartnershipSettlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomSettlementPeriodDays",
                table: "ProducerPartnershipAgreements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumSettlementAmount",
                table: "ProducerPartnershipAgreements",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProducerPartnershipSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgreementId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GrossRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RefundDeductions = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PlatformFeePercentageApplied = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    PlatformFeeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetPartnershipRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProducerSharePercentageApplied = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    BusinessPartnerSharePercentageApplied = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ProducerShareAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BusinessPartnerShareAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OrderCount = table.Column<int>(type: "integer", nullable: false),
                    BelowMinimumThreshold = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PayoutReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerPartnershipSettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipSettlements_ProducerPartnershipAgreement~",
                        column: x => x.AgreementId,
                        principalTable: "ProducerPartnershipAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProducerPartnershipSettlements_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipSettlements_AgreementId",
                table: "ProducerPartnershipSettlements",
                column: "AgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipSettlements_AgreementId_PeriodStart_Peri~",
                table: "ProducerPartnershipSettlements",
                columns: new[] { "AgreementId", "PeriodStart", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipSettlements_ApprovedByUserId",
                table: "ProducerPartnershipSettlements",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerPartnershipSettlements_Status",
                table: "ProducerPartnershipSettlements",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProducerPartnershipSettlements");

            migrationBuilder.DropColumn(
                name: "CustomSettlementPeriodDays",
                table: "ProducerPartnershipAgreements");

            migrationBuilder.DropColumn(
                name: "MinimumSettlementAmount",
                table: "ProducerPartnershipAgreements");
        }
    }
}

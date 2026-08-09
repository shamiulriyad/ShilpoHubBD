using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestmentOpportunities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProjectDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    FundingRequirement = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentOpportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentOpportunities_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentProposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvestmentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProposalMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentProposals_InvestmentOpportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "InvestmentOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestmentProposals_Users_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentDocuments_InvestmentProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "InvestmentProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentMilestones_InvestmentProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "InvestmentProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentStatusEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentStatusEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentStatusEvents_InvestmentProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "InvestmentProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentDocuments_ProposalId",
                table: "InvestmentDocuments",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentMilestones_ProposalId",
                table: "InvestmentMilestones",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOpportunities_ProducerId",
                table: "InvestmentOpportunities",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentProposals_BusinessPartnerId",
                table: "InvestmentProposals",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentProposals_OpportunityId",
                table: "InvestmentProposals",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentStatusEvents_ProposalId",
                table: "InvestmentStatusEvents",
                column: "ProposalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestmentDocuments");

            migrationBuilder.DropTable(
                name: "InvestmentMilestones");

            migrationBuilder.DropTable(
                name: "InvestmentStatusEvents");

            migrationBuilder.DropTable(
                name: "InvestmentProposals");

            migrationBuilder.DropTable(
                name: "InvestmentOpportunities");
        }
    }
}

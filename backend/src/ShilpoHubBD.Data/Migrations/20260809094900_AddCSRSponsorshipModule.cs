using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCSRSponsorshipModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SponsorshipOpportunities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    FundingGoal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorshipOpportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsorshipOpportunities_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SponsorshipProposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundingAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_SponsorshipProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsorshipProposals_SponsorshipOpportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "SponsorshipOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SponsorshipProposals_Users_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SponsorshipImpactRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Metric = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorshipImpactRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsorshipImpactRecords_SponsorshipProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "SponsorshipProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SponsorshipMilestones",
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
                    table.PrimaryKey("PK_SponsorshipMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsorshipMilestones_SponsorshipProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "SponsorshipProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SponsorshipProgressUpdates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorshipProgressUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsorshipProgressUpdates_SponsorshipProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "SponsorshipProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SponsorshipProgressUpdates_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SponsorshipStatusEvents",
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
                    table.PrimaryKey("PK_SponsorshipStatusEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SponsorshipStatusEvents_SponsorshipProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "SponsorshipProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipImpactRecords_ProposalId",
                table: "SponsorshipImpactRecords",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipMilestones_ProposalId",
                table: "SponsorshipMilestones",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipOpportunities_ProducerId",
                table: "SponsorshipOpportunities",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipProgressUpdates_AuthorUserId",
                table: "SponsorshipProgressUpdates",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipProgressUpdates_ProposalId",
                table: "SponsorshipProgressUpdates",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipProposals_BusinessPartnerId",
                table: "SponsorshipProposals",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipProposals_OpportunityId",
                table: "SponsorshipProposals",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorshipStatusEvents_ProposalId",
                table: "SponsorshipStatusEvents",
                column: "ProposalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SponsorshipImpactRecords");

            migrationBuilder.DropTable(
                name: "SponsorshipMilestones");

            migrationBuilder.DropTable(
                name: "SponsorshipProgressUpdates");

            migrationBuilder.DropTable(
                name: "SponsorshipStatusEvents");

            migrationBuilder.DropTable(
                name: "SponsorshipProposals");

            migrationBuilder.DropTable(
                name: "SponsorshipOpportunities");
        }
    }
}

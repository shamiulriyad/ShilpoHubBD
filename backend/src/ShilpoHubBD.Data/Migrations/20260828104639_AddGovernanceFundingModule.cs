using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGovernanceFundingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FundingPrograms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EligibilityCriteria = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    TotalBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DisbursedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MinAmountPerApplicant = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxAmountPerApplicant = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ApplicationOpensAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApplicationClosesAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequiresRepayment = table.Column<bool>(type: "boolean", nullable: false),
                    InterestRatePercent = table.Column<decimal>(type: "numeric(6,3)", nullable: true),
                    RepaymentPeriodMonths = table.Column<int>(type: "integer", nullable: true),
                    ManagedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundingPrograms_Users_ManagedByUserId",
                        column: x => x.ManagedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FundingApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FundingProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ApplicantType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApplicantUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicantVillageId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicantLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Purpose = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Justification = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ContactName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecisionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RepaymentStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalRepaid = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NextRepaymentDueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundingApplications_FundingPrograms_FundingProgramId",
                        column: x => x.FundingProgramId,
                        principalTable: "FundingPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingApplications_Users_ApplicantUserId",
                        column: x => x.ApplicantUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FundingApplications_Users_DecisionByUserId",
                        column: x => x.DecisionByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FundingApplications_Villages_ApplicantVillageId",
                        column: x => x.ApplicantVillageId,
                        principalTable: "Villages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FundingApplicationEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FundingApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingApplicationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundingApplicationEvents_FundingApplications_FundingApplica~",
                        column: x => x.FundingApplicationId,
                        principalTable: "FundingApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingApplicationEvents_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FundingApplicationReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FundingApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    RecommendedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Comments = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingApplicationReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundingApplicationReviews_FundingApplications_FundingApplic~",
                        column: x => x.FundingApplicationId,
                        principalTable: "FundingApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingApplicationReviews_Users_ReviewerUserId",
                        column: x => x.ReviewerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FundingDisbursements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FundingApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingDisbursements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundingDisbursements_FundingApplications_FundingApplication~",
                        column: x => x.FundingApplicationId,
                        principalTable: "FundingApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingDisbursements_Users_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplicationEvents_ActorUserId",
                table: "FundingApplicationEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplicationEvents_FundingApplicationId_CreatedAt",
                table: "FundingApplicationEvents",
                columns: new[] { "FundingApplicationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplicationReviews_FundingApplicationId_CreatedAt",
                table: "FundingApplicationReviews",
                columns: new[] { "FundingApplicationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplicationReviews_ReviewerUserId",
                table: "FundingApplicationReviews",
                column: "ReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_ApplicantUserId",
                table: "FundingApplications",
                column: "ApplicantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_ApplicantVillageId",
                table: "FundingApplications",
                column: "ApplicantVillageId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_DecisionByUserId",
                table: "FundingApplications",
                column: "DecisionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_FundingProgramId_Status",
                table: "FundingApplications",
                columns: new[] { "FundingProgramId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_ReferenceCode",
                table: "FundingApplications",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_RepaymentStatus",
                table: "FundingApplications",
                column: "RepaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_Status",
                table: "FundingApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_SubmittedAt",
                table: "FundingApplications",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FundingDisbursements_FundingApplicationId_ScheduledFor",
                table: "FundingDisbursements",
                columns: new[] { "FundingApplicationId", "ScheduledFor" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingDisbursements_RecordedByUserId",
                table: "FundingDisbursements",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingDisbursements_Status",
                table: "FundingDisbursements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FundingPrograms_ManagedByUserId",
                table: "FundingPrograms",
                column: "ManagedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingPrograms_Slug",
                table: "FundingPrograms",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundingPrograms_Status",
                table: "FundingPrograms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FundingPrograms_Type",
                table: "FundingPrograms",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FundingApplicationEvents");

            migrationBuilder.DropTable(
                name: "FundingApplicationReviews");

            migrationBuilder.DropTable(
                name: "FundingDisbursements");

            migrationBuilder.DropTable(
                name: "FundingApplications");

            migrationBuilder.DropTable(
                name: "FundingPrograms");
        }
    }
}

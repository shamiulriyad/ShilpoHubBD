using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArtisanSupportWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArtisanSupportCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ArtisanUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByAdminUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProblemTitle = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    ProblemDescription = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    District = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Craft = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AssignedOfficerName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    AssignedOfficerPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    InspectionResult = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IdentityFindings = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    WorkshopFindings = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    CraftAuthenticityFindings = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    ProductToolsFindings = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    ProblemFindings = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    RootCause = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    SupportPlan = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    SupportKind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FundingSourceType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    FundingSourceName = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    SupportAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SupportValueDescription = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    SupportProvidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArtisanConfirmation = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ArtisanConfirmationNotes = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    HasDispute = table.Column<bool>(type: "boolean", nullable: false),
                    IsFlagged = table.Column<bool>(type: "boolean", nullable: false),
                    FlagReason = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    MonitoringDueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReportDueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtisanSupportCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportCases_Users_ArtisanUserId",
                        column: x => x.ArtisanUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportCases_Users_AssignedByAdminUserId",
                        column: x => x.AssignedByAdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportCases_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportCases_Users_OrganizationUserId",
                        column: x => x.OrganizationUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupportOrganizationProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrganizationType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    RegistrationAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RegistrationDocumentUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OfficialEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OfficialPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OperatingDistrictsJson = table.Column<string>(type: "jsonb", nullable: false),
                    OrganizationDetails = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RepresentativeName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    RepresentativeDesignation = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    RepresentativeNid = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RepresentativePhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportOrganizationProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportOrganizationProfiles_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupportOrganizationProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArtisanSupportEvidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stage = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtisanSupportEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportEvidence_ArtisanSupportCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ArtisanSupportCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportEvidence_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArtisanSupportMonitoring",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductionStatus = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IncomeMarketImprovement = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EquipmentCondition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProblemSolved = table.Column<bool>(type: "boolean", nullable: false),
                    NewIssues = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    FollowedUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtisanSupportMonitoring", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportMonitoring_ArtisanSupportCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ArtisanSupportCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportMonitoring_Users_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArtisanSupportReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalProblem = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    VerificationFindings = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    ActionTaken = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    FundingSupportSource = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    AmountValueUsed = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ReceiptsEvidenceSummary = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    ArtisanConfirmationSummary = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    MonitoringResult = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    FinalOutcome = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    Recommendation = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    ReviewStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AdminReviewNotes = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtisanSupportReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportReports_ArtisanSupportCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ArtisanSupportCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportReports_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ArtisanSupportReports_Users_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportCases_ArtisanUserId_Status",
                table: "ArtisanSupportCases",
                columns: new[] { "ArtisanUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportCases_AssignedByAdminUserId",
                table: "ArtisanSupportCases",
                column: "AssignedByAdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportCases_CaseNumber",
                table: "ArtisanSupportCases",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportCases_CreatedByUserId",
                table: "ArtisanSupportCases",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportCases_HasDispute",
                table: "ArtisanSupportCases",
                column: "HasDispute");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportCases_OrganizationUserId_Status",
                table: "ArtisanSupportCases",
                columns: new[] { "OrganizationUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportCases_ReportDueAt",
                table: "ArtisanSupportCases",
                column: "ReportDueAt");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportEvidence_CaseId",
                table: "ArtisanSupportEvidence",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportEvidence_UploadedByUserId",
                table: "ArtisanSupportEvidence",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportMonitoring_CaseId",
                table: "ArtisanSupportMonitoring",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportMonitoring_RecordedByUserId",
                table: "ArtisanSupportMonitoring",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportReports_CaseId",
                table: "ArtisanSupportReports",
                column: "CaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportReports_ReviewedByUserId",
                table: "ArtisanSupportReports",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportReports_ReviewStatus",
                table: "ArtisanSupportReports",
                column: "ReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ArtisanSupportReports_SubmittedByUserId",
                table: "ArtisanSupportReports",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportOrganizationProfiles_RegistrationNumber",
                table: "SupportOrganizationProfiles",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportOrganizationProfiles_ReviewedByUserId",
                table: "SupportOrganizationProfiles",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportOrganizationProfiles_UserId",
                table: "SupportOrganizationProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtisanSupportEvidence");

            migrationBuilder.DropTable(
                name: "ArtisanSupportMonitoring");

            migrationBuilder.DropTable(
                name: "ArtisanSupportReports");

            migrationBuilder.DropTable(
                name: "SupportOrganizationProfiles");

            migrationBuilder.DropTable(
                name: "ArtisanSupportCases");
        }
    }
}

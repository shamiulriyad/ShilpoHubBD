using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGovernanceMonitoringModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplianceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Framework = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OverallScorePercent = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextReviewDue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceRecords_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplianceRecords_Users_ReviewerUserId",
                        column: x => x.ReviewerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringFlags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlagType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubjectLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EvidenceJson = table.Column<string>(type: "text", nullable: true),
                    RiskScore = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    DedupeKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringFlags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoringFlags_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MonitoringFlags_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonitoringFlags_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplianceRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Evidence = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceRequirements_ComplianceRecords_ComplianceRecordId",
                        column: x => x.ComplianceRecordId,
                        principalTable: "ComplianceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Complaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ComplainantUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComplainantName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ComplainantContact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AgainstType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AgainstId = table.Column<Guid>(type: "uuid", nullable: true),
                    AgainstLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RelatedOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    MonitoringFlagId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Resolution = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Complaints_MonitoringFlags_MonitoringFlagId",
                        column: x => x.MonitoringFlagId,
                        principalTable: "MonitoringFlags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Complaints_Orders_RelatedOrderId",
                        column: x => x.RelatedOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Complaints_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Complaints_Users_ComplainantUserId",
                        column: x => x.ComplainantUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Complaints_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringFlagEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonitoringFlagId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringFlagEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoringFlagEvents_MonitoringFlags_MonitoringFlagId",
                        column: x => x.MonitoringFlagId,
                        principalTable: "MonitoringFlags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonitoringFlagEvents_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintUpdates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsInternal = table.Column<bool>(type: "boolean", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintUpdates_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplaintUpdates_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_AssignedToUserId",
                table: "Complaints",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_Category",
                table: "Complaints",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ComplainantUserId",
                table: "Complaints",
                column: "ComplainantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CreatedAt",
                table: "Complaints",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_MonitoringFlagId",
                table: "Complaints",
                column: "MonitoringFlagId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_Priority",
                table: "Complaints",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ReferenceCode",
                table: "Complaints",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_RelatedOrderId",
                table: "Complaints",
                column: "RelatedOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ResolvedByUserId",
                table: "Complaints",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_Status",
                table: "Complaints",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintUpdates_ActorUserId",
                table: "ComplaintUpdates",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintUpdates_ComplaintId_CreatedAt",
                table: "ComplaintUpdates",
                columns: new[] { "ComplaintId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRecords_CreatedByUserId",
                table: "ComplianceRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRecords_EntityType_EntityId",
                table: "ComplianceRecords",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRecords_NextReviewDue",
                table: "ComplianceRecords",
                column: "NextReviewDue");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRecords_ReviewerUserId",
                table: "ComplianceRecords",
                column: "ReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRecords_Status",
                table: "ComplianceRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRequirements_ComplianceRecordId_DisplayOrder",
                table: "ComplianceRequirements",
                columns: new[] { "ComplianceRecordId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringFlagEvents_ActorUserId",
                table: "MonitoringFlagEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringFlagEvents_MonitoringFlagId_CreatedAt",
                table: "MonitoringFlagEvents",
                columns: new[] { "MonitoringFlagId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringFlags_AssignedToUserId",
                table: "MonitoringFlags",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringFlags_CreatedByUserId",
                table: "MonitoringFlags",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringFlags_DedupeKey",
                table: "MonitoringFlags",
                column: "DedupeKey");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringFlags_DetectedAt",
                table: "MonitoringFlags",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringFlags_FlagType",
                table: "MonitoringFlags",
                column: "FlagType");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringFlags_ResolvedByUserId",
                table: "MonitoringFlags",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringFlags_Severity",
                table: "MonitoringFlags",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringFlags_Status",
                table: "MonitoringFlags",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringFlags_SubjectType_SubjectId",
                table: "MonitoringFlags",
                columns: new[] { "SubjectType", "SubjectId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplaintUpdates");

            migrationBuilder.DropTable(
                name: "ComplianceRequirements");

            migrationBuilder.DropTable(
                name: "MonitoringFlagEvents");

            migrationBuilder.DropTable(
                name: "Complaints");

            migrationBuilder.DropTable(
                name: "ComplianceRecords");

            migrationBuilder.DropTable(
                name: "MonitoringFlags");
        }
    }
}

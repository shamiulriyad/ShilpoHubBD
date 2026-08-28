using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGovernanceHeritageIntelligenceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeritageIndexRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndexType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScopeLabel = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Score = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    Rating = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Method = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignalsJson = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageIndexRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageIndexRecords_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HeritageIndexComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageIndexRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    RawValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    ContributionScore = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    Detail = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageIndexComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageIndexComponents_HeritageIndexRecords_HeritageIndexR~",
                        column: x => x.HeritageIndexRecordId,
                        principalTable: "HeritageIndexRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HeritageIndexComponents_HeritageIndexRecordId_DisplayOrder",
                table: "HeritageIndexComponents",
                columns: new[] { "HeritageIndexRecordId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_HeritageIndexRecords_GeneratedByUserId",
                table: "HeritageIndexRecords",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageIndexRecords_IndexType",
                table: "HeritageIndexRecords",
                column: "IndexType");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageIndexRecords_IndexType_Scope_ScopeId_PeriodEnd",
                table: "HeritageIndexRecords",
                columns: new[] { "IndexType", "Scope", "ScopeId", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_HeritageIndexRecords_ScopeLabel",
                table: "HeritageIndexRecords",
                column: "ScopeLabel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeritageIndexComponents");

            migrationBuilder.DropTable(
                name: "HeritageIndexRecords");
        }
    }
}

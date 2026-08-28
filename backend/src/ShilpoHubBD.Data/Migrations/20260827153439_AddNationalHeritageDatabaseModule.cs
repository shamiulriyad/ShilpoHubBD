using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNationalHeritageDatabaseModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeritageRiskRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CraftName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    VillageId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: true),
                    AffectedArtisanCount = table.Column<int>(type: "integer", nullable: true),
                    ContributingFactors = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RecommendedActions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    AssessmentYear = table.Column<int>(type: "integer", nullable: true),
                    AssessedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageRiskRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageRiskRecords_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HeritageRiskRecords_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HeritageRiskRecords_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HeritageRiskRecords_Villages_VillageId",
                        column: x => x.VillageId,
                        principalTable: "Villages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HeritageDatasetAccessGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageDatasetId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessRole = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageDatasetAccessGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageDatasetAccessGrants_Users_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HeritageDatasetAccessGrants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HeritageDatasetExports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageDatasetId = table.Column<Guid>(type: "uuid", nullable: false),
                    DatasetVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RowCount = table.Column<int>(type: "integer", nullable: false),
                    FilterJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageDatasetExports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageDatasetExports_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HeritageDatasets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AccessLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceOrganization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SourceReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    License = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsLive = table.Column<bool>(type: "boolean", nullable: false),
                    RecordCount = table.Column<int>(type: "integer", nullable: false),
                    VersionCount = table.Column<int>(type: "integer", nullable: false),
                    DataUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRefreshedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageDatasets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageDatasets_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HeritageDatasetVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageDatasetId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Changelog = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RecordCount = table.Column<int>(type: "integer", nullable: false),
                    Format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    SourceFileUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SourceContentHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ImportedRowCount = table.Column<int>(type: "integer", nullable: true),
                    ImportErrorCount = table.Column<int>(type: "integer", nullable: false),
                    ImportNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SchemaJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageDatasetVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageDatasetVersions_HeritageDatasets_HeritageDatasetId",
                        column: x => x.HeritageDatasetId,
                        principalTable: "HeritageDatasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HeritageDatasetVersions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasetAccessGrants_GrantedByUserId",
                table: "HeritageDatasetAccessGrants",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasetAccessGrants_HeritageDatasetId_UserId",
                table: "HeritageDatasetAccessGrants",
                columns: new[] { "HeritageDatasetId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasetAccessGrants_UserId",
                table: "HeritageDatasetAccessGrants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasetExports_DatasetVersionId",
                table: "HeritageDatasetExports",
                column: "DatasetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasetExports_HeritageDatasetId",
                table: "HeritageDatasetExports",
                column: "HeritageDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasetExports_HeritageDatasetId_CreatedAt",
                table: "HeritageDatasetExports",
                columns: new[] { "HeritageDatasetId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasetExports_RequestedByUserId",
                table: "HeritageDatasetExports",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasets_AccessLevel",
                table: "HeritageDatasets",
                column: "AccessLevel");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasets_Category",
                table: "HeritageDatasets",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasets_CurrentVersionId",
                table: "HeritageDatasets",
                column: "CurrentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasets_OwnerUserId",
                table: "HeritageDatasets",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasets_Slug",
                table: "HeritageDatasets",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasets_Status",
                table: "HeritageDatasets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasetVersions_CreatedByUserId",
                table: "HeritageDatasetVersions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageDatasetVersions_HeritageDatasetId_VersionNumber",
                table: "HeritageDatasetVersions",
                columns: new[] { "HeritageDatasetId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeritageRiskRecords_AssessmentYear",
                table: "HeritageRiskRecords",
                column: "AssessmentYear");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageRiskRecords_Category",
                table: "HeritageRiskRecords",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageRiskRecords_CreatedByUserId",
                table: "HeritageRiskRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageRiskRecords_DistrictId",
                table: "HeritageRiskRecords",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageRiskRecords_Level",
                table: "HeritageRiskRecords",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageRiskRecords_ProducerId",
                table: "HeritageRiskRecords",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageRiskRecords_VillageId",
                table: "HeritageRiskRecords",
                column: "VillageId");

            migrationBuilder.AddForeignKey(
                name: "FK_HeritageDatasetAccessGrants_HeritageDatasets_HeritageDatase~",
                table: "HeritageDatasetAccessGrants",
                column: "HeritageDatasetId",
                principalTable: "HeritageDatasets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HeritageDatasetExports_HeritageDatasetVersions_DatasetVersi~",
                table: "HeritageDatasetExports",
                column: "DatasetVersionId",
                principalTable: "HeritageDatasetVersions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HeritageDatasetExports_HeritageDatasets_HeritageDatasetId",
                table: "HeritageDatasetExports",
                column: "HeritageDatasetId",
                principalTable: "HeritageDatasets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HeritageDatasets_HeritageDatasetVersions_CurrentVersionId",
                table: "HeritageDatasets",
                column: "CurrentVersionId",
                principalTable: "HeritageDatasetVersions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HeritageDatasetVersions_HeritageDatasets_HeritageDatasetId",
                table: "HeritageDatasetVersions");

            migrationBuilder.DropTable(
                name: "HeritageDatasetAccessGrants");

            migrationBuilder.DropTable(
                name: "HeritageDatasetExports");

            migrationBuilder.DropTable(
                name: "HeritageRiskRecords");

            migrationBuilder.DropTable(
                name: "HeritageDatasets");

            migrationBuilder.DropTable(
                name: "HeritageDatasetVersions");
        }
    }
}

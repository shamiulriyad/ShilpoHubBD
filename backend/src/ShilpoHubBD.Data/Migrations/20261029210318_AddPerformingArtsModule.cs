using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformingArtsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerformingArtRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleBn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AlternateNames = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SummaryEn = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SummaryBn = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DescriptionEn = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    DescriptionBn = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    RegionalContextEn = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RegionalContextBn = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Division = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    Upazila = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Communities = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Artists = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Instruments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ViabilityNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InventoryReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReviewNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformingArtRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformingArtRecords_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PerformingArtMedia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CaptionEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CaptionBn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Credit = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    License = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SourceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformingArtMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformingArtMedia_PerformingArtRecords_RecordId",
                        column: x => x.RecordId,
                        principalTable: "PerformingArtRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformingArtSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Publisher = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AccessedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformingArtSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformingArtSources_PerformingArtRecords_RecordId",
                        column: x => x.RecordId,
                        principalTable: "PerformingArtRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerformingArtMedia_RecordId_DisplayOrder",
                table: "PerformingArtMedia",
                columns: new[] { "RecordId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformingArtRecords_DistrictId",
                table: "PerformingArtRecords",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformingArtRecords_Slug",
                table: "PerformingArtRecords",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformingArtRecords_Status_Category",
                table: "PerformingArtRecords",
                columns: new[] { "Status", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformingArtRecords_Status_Division",
                table: "PerformingArtRecords",
                columns: new[] { "Status", "Division" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformingArtRecords_Status_IsFeatured",
                table: "PerformingArtRecords",
                columns: new[] { "Status", "IsFeatured" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformingArtSources_RecordId_DisplayOrder",
                table: "PerformingArtSources",
                columns: new[] { "RecordId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerformingArtMedia");

            migrationBuilder.DropTable(
                name: "PerformingArtSources");

            migrationBuilder.DropTable(
                name: "PerformingArtRecords");
        }
    }
}

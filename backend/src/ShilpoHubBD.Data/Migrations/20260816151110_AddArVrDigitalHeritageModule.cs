using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArVrDigitalHeritageModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CulturalStories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CoverImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HeritagePlaceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CulturalStories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CulturalStories_HeritagePlaces_HeritagePlaceId",
                        column: x => x.HeritagePlaceId,
                        principalTable: "HeritagePlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MuseumItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Era = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ModelUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuseumItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuseumItems_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MuseumItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VillageTourStops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MediaUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MediaType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HeritagePlaceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VillageTourStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VillageTourStops_HeritagePlaces_HeritagePlaceId",
                        column: x => x.HeritagePlaceId,
                        principalTable: "HeritagePlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CulturalStoryChapters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Heading = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    MediaUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MediaType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CulturalStoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CulturalStoryChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CulturalStoryChapters_CulturalStories_CulturalStoryId",
                        column: x => x.CulturalStoryId,
                        principalTable: "CulturalStories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MuseumItemMedia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MediaType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Caption = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    MuseumItemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuseumItemMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuseumItemMedia_MuseumItems_MuseumItemId",
                        column: x => x.MuseumItemId,
                        principalTable: "MuseumItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CulturalStories_HeritagePlaceId",
                table: "CulturalStories",
                column: "HeritagePlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_CulturalStories_IsActive",
                table: "CulturalStories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CulturalStoryChapters_CulturalStoryId",
                table: "CulturalStoryChapters",
                column: "CulturalStoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MuseumItemMedia_MuseumItemId",
                table: "MuseumItemMedia",
                column: "MuseumItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MuseumItems_Category",
                table: "MuseumItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_MuseumItems_DistrictId",
                table: "MuseumItems",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_MuseumItems_IsActive",
                table: "MuseumItems",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MuseumItems_ProductId",
                table: "MuseumItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_VillageTourStops_HeritagePlaceId",
                table: "VillageTourStops",
                column: "HeritagePlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_VillageTourStops_IsActive",
                table: "VillageTourStops",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CulturalStoryChapters");

            migrationBuilder.DropTable(
                name: "MuseumItemMedia");

            migrationBuilder.DropTable(
                name: "VillageTourStops");

            migrationBuilder.DropTable(
                name: "CulturalStories");

            migrationBuilder.DropTable(
                name: "MuseumItems");
        }
    }
}

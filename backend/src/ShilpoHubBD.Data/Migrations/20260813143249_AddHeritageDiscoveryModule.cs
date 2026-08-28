using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeritageDiscoveryModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeritagePlaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    PlaceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritagePlaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritagePlaces_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CulturalEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritagePlaceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CulturalEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CulturalEvents_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CulturalEvents_HeritagePlaces_HeritagePlaceId",
                        column: x => x.HeritagePlaceId,
                        principalTable: "HeritagePlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HeritageFestivals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRecurringAnnually = table.Column<bool>(type: "boolean", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritagePlaceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageFestivals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageFestivals_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HeritageFestivals_HeritagePlaces_HeritagePlaceId",
                        column: x => x.HeritagePlaceId,
                        principalTable: "HeritagePlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LocalCuisines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    WhereToTry = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritagePlaceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalCuisines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalCuisines_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocalCuisines_HeritagePlaces_HeritagePlaceId",
                        column: x => x.HeritagePlaceId,
                        principalTable: "HeritagePlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CulturalEvents_Category",
                table: "CulturalEvents",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CulturalEvents_DistrictId",
                table: "CulturalEvents",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_CulturalEvents_EventDate",
                table: "CulturalEvents",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_CulturalEvents_HeritagePlaceId",
                table: "CulturalEvents",
                column: "HeritagePlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_CulturalEvents_IsActive",
                table: "CulturalEvents",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageFestivals_DistrictId",
                table: "HeritageFestivals",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageFestivals_HeritagePlaceId",
                table: "HeritageFestivals",
                column: "HeritagePlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageFestivals_IsActive",
                table: "HeritageFestivals",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageFestivals_StartDate",
                table: "HeritageFestivals",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_HeritagePlaces_DistrictId",
                table: "HeritagePlaces",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritagePlaces_IsActive",
                table: "HeritagePlaces",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HeritagePlaces_Latitude_Longitude",
                table: "HeritagePlaces",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_HeritagePlaces_PlaceType",
                table: "HeritagePlaces",
                column: "PlaceType");

            migrationBuilder.CreateIndex(
                name: "IX_LocalCuisines_DistrictId",
                table: "LocalCuisines",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalCuisines_HeritagePlaceId",
                table: "LocalCuisines",
                column: "HeritagePlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalCuisines_IsActive",
                table: "LocalCuisines",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CulturalEvents");

            migrationBuilder.DropTable(
                name: "HeritageFestivals");

            migrationBuilder.DropTable(
                name: "LocalCuisines");

            migrationBuilder.DropTable(
                name: "HeritagePlaces");
        }
    }
}

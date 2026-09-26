using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTourismExternalSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "TourismLocations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "TourismLocations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "TourismLocations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Admin");

            migrationBuilder.AddColumn<string>(
                name: "Upazila",
                table: "TourismLocations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourismLocations_Source_ExternalId",
                table: "TourismLocations",
                columns: new[] { "Source", "ExternalId" },
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TourismLocations_Source_ExternalId",
                table: "TourismLocations");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "TourismLocations");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "TourismLocations");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "TourismLocations");

            migrationBuilder.DropColumn(
                name: "Upazila",
                table: "TourismLocations");
        }
    }
}

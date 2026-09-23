using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTourismLocationProvenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "TourismLocations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoordinatesPrecision",
                table: "TourismLocations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoordinatesSource",
                table: "TourismLocations",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataRetrievedOn",
                table: "TourismLocations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "TourismLocations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnverifiedFields",
                table: "TourismLocations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationStatus",
                table: "TourismLocations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unverified");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "TourismLocations");

            migrationBuilder.DropColumn(
                name: "CoordinatesPrecision",
                table: "TourismLocations");

            migrationBuilder.DropColumn(
                name: "CoordinatesSource",
                table: "TourismLocations");

            migrationBuilder.DropColumn(
                name: "DataRetrievedOn",
                table: "TourismLocations");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "TourismLocations");

            migrationBuilder.DropColumn(
                name: "UnverifiedFields",
                table: "TourismLocations");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "TourismLocations");
        }
    }
}

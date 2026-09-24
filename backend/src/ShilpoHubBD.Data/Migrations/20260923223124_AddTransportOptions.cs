using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransportOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OriginName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DestinationDistrict = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Operator = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ServiceClasses = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Schedule = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DurationText = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FareBdt = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    FareNote = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    BookingUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SourceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    VerificationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UnverifiedFields = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataRetrievedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportOptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransportOptions_DestinationDistrict_Mode",
                table: "TransportOptions",
                columns: new[] { "DestinationDistrict", "Mode" });

            migrationBuilder.CreateIndex(
                name: "IX_TransportOptions_OriginName_DestinationDistrict_Mode_Servic~",
                table: "TransportOptions",
                columns: new[] { "OriginName", "DestinationDistrict", "Mode", "ServiceName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransportOptions");
        }
    }
}

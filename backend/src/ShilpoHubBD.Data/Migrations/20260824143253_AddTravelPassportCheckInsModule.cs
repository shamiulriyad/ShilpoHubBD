using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelPassportCheckInsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProductId_UserId",
                table: "Reviews");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "TouristServices",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "TouristServices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "Reviews",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "BookingId",
                table: "Reviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HeritagePlaceId",
                table: "Reviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "HeritagePlaces",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "HeritagePlaces",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequiredCheckInCount",
                table: "Badges",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HeritageCheckIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritagePlaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    CheckInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageCheckIns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageCheckIns_HeritagePlaces_HeritagePlaceId",
                        column: x => x.HeritagePlaceId,
                        principalTable: "HeritagePlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HeritageCheckIns_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeritageRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    TotalDistanceKm = table.Column<double>(type: "double precision", nullable: false),
                    IsRecommended = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TravelJournalEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    PhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HeritagePlaceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckInId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelJournalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelJournalEntries_HeritageCheckIns_CheckInId",
                        column: x => x.CheckInId,
                        principalTable: "HeritageCheckIns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TravelJournalEntries_HeritagePlaces_HeritagePlaceId",
                        column: x => x.HeritagePlaceId,
                        principalTable: "HeritagePlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TravelJournalEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteStops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritagePlaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    DistanceFromPreviousKm = table.Column<double>(type: "double precision", nullable: true),
                    EstimatedTravelMinutesFromPrevious = table.Column<int>(type: "integer", nullable: true),
                    TransportationMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteStops_HeritagePlaces_HeritagePlaceId",
                        column: x => x.HeritagePlaceId,
                        principalTable: "HeritagePlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteStops_HeritageRoutes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "HeritageRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId_UserId",
                table: "Reviews",
                columns: new[] { "BookingId", "UserId" },
                unique: true,
                filter: "\"BookingId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_HeritagePlaceId_UserId",
                table: "Reviews",
                columns: new[] { "HeritagePlaceId", "UserId" },
                unique: true,
                filter: "\"HeritagePlaceId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId_UserId",
                table: "Reviews",
                columns: new[] { "ProductId", "UserId" },
                unique: true,
                filter: "\"ProductId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageCheckIns_HeritagePlaceId",
                table: "HeritageCheckIns",
                column: "HeritagePlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageCheckIns_UserId_HeritagePlaceId_CheckInDate",
                table: "HeritageCheckIns",
                columns: new[] { "UserId", "HeritagePlaceId", "CheckInDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeritageRoutes_IsRecommended",
                table: "HeritageRoutes",
                column: "IsRecommended");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageRoutes_Status",
                table: "HeritageRoutes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_HeritagePlaceId",
                table: "RouteStops",
                column: "HeritagePlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_RouteId_HeritagePlaceId",
                table: "RouteStops",
                columns: new[] { "RouteId", "HeritagePlaceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_RouteId_Order",
                table: "RouteStops",
                columns: new[] { "RouteId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TravelJournalEntries_CheckInId",
                table: "TravelJournalEntries",
                column: "CheckInId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelJournalEntries_HeritagePlaceId",
                table: "TravelJournalEntries",
                column: "HeritagePlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelJournalEntries_UserId",
                table: "TravelJournalEntries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Bookings_BookingId",
                table: "Reviews",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_HeritagePlaces_HeritagePlaceId",
                table: "Reviews",
                column: "HeritagePlaceId",
                principalTable: "HeritagePlaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Bookings_BookingId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_HeritagePlaces_HeritagePlaceId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "RouteStops");

            migrationBuilder.DropTable(
                name: "TravelJournalEntries");

            migrationBuilder.DropTable(
                name: "HeritageRoutes");

            migrationBuilder.DropTable(
                name: "HeritageCheckIns");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_BookingId_UserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_HeritagePlaceId_UserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProductId_UserId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "TouristServices");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "TouristServices");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "HeritagePlaceId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "HeritagePlaces");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "HeritagePlaces");

            migrationBuilder.DropColumn(
                name: "RequiredCheckInCount",
                table: "Badges");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "Reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId_UserId",
                table: "Reviews",
                columns: new[] { "ProductId", "UserId" },
                unique: true);
        }
    }
}

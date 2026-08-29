using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLogisticsRouteOptimizationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    LogisticsPartnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartLocationLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StartLatitude = table.Column<double>(type: "double precision", nullable: true),
                    StartLongitude = table.Column<double>(type: "double precision", nullable: true),
                    EndLocationLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EndLatitude = table.Column<double>(type: "double precision", nullable: true),
                    EndLongitude = table.Column<double>(type: "double precision", nullable: true),
                    OriginDistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedDriverName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    AssignedDriverPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    AssignedVehicleLabel = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    VehicleCapacityKg = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalStops = table.Column<int>(type: "integer", nullable: false),
                    CompletedStops = table.Column<int>(type: "integer", nullable: false),
                    TotalLoadKg = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    TotalDistanceKm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    OptimizationStrategy = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryRoutes_Districts_OriginDistrictId",
                        column: x => x.OriginDistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeliveryRoutes_LogisticsPartnerProfiles_LogisticsPartnerPro~",
                        column: x => x.LogisticsPartnerProfileId,
                        principalTable: "LogisticsPartnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryRoutes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryRouteEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RouteStopId = table.Column<Guid>(type: "uuid", nullable: true),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRouteEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryRouteEvents_DeliveryRoutes_DeliveryRouteId",
                        column: x => x.DeliveryRouteId,
                        principalTable: "DeliveryRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryRouteEvents_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryRouteStops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    StopType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PickupRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    AddressLine = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    City = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    LoadKg = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    PackageCount = table.Column<int>(type: "integer", nullable: false),
                    PlannedArrivalAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedDepartureAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualArrivalAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualDepartureAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ServiceDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    DistanceFromPreviousKm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CompletionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRouteStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryRouteStops_DeliveryRoutes_DeliveryRouteId",
                        column: x => x.DeliveryRouteId,
                        principalTable: "DeliveryRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryRouteStops_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeliveryRouteStops_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeliveryRouteStops_PickupRequests_PickupRequestId",
                        column: x => x.PickupRequestId,
                        principalTable: "PickupRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRouteEvents_ActorUserId",
                table: "DeliveryRouteEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRouteEvents_DeliveryRouteId_CreatedAt",
                table: "DeliveryRouteEvents",
                columns: new[] { "DeliveryRouteId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRoutes_CreatedByUserId",
                table: "DeliveryRoutes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRoutes_LogisticsPartnerProfileId_Status",
                table: "DeliveryRoutes",
                columns: new[] { "LogisticsPartnerProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRoutes_OriginDistrictId",
                table: "DeliveryRoutes",
                column: "OriginDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRoutes_RouteCode",
                table: "DeliveryRoutes",
                column: "RouteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRoutes_ScheduledDate",
                table: "DeliveryRoutes",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRouteStops_DeliveryRouteId_Sequence",
                table: "DeliveryRouteStops",
                columns: new[] { "DeliveryRouteId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRouteStops_DistrictId",
                table: "DeliveryRouteStops",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRouteStops_OrderId",
                table: "DeliveryRouteStops",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRouteStops_PickupRequestId",
                table: "DeliveryRouteStops",
                column: "PickupRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRouteStops_Status",
                table: "DeliveryRouteStops",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryRouteEvents");

            migrationBuilder.DropTable(
                name: "DeliveryRouteStops");

            migrationBuilder.DropTable(
                name: "DeliveryRoutes");
        }
    }
}

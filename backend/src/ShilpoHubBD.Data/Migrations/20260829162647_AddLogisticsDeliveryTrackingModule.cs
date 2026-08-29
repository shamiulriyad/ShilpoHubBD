using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLogisticsDeliveryTrackingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackingNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    LogisticsPartnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ServiceLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    PickupRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeliveryRouteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginContactName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    OriginPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OriginAddressLine = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    OriginCity = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    OriginDistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginPostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    RecipientPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DestinationAddressLine = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    DestinationCity = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DestinationDistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationPostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ParcelCount = table.Column<int>(type: "integer", nullable: false),
                    TotalWeightKg = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    DimensionsNote = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    DeclaredValue = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    ShippingCost = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    IsCashOnDelivery = table.Column<bool>(type: "boolean", nullable: false),
                    CodAmount = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    CodCollected = table.Column<bool>(type: "boolean", nullable: false),
                    CodCollectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentLocationLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CurrentLatitude = table.Column<double>(type: "double precision", nullable: true),
                    CurrentLongitude = table.Column<double>(type: "double precision", nullable: true),
                    EstimatedDeliveryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DispatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastStatusAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    ReceivedByName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ProofOfDeliveryNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SignatureImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_DeliveryRoutes_DeliveryRouteId",
                        column: x => x.DeliveryRouteId,
                        principalTable: "DeliveryRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Shipments_Districts_DestinationDistrictId",
                        column: x => x.DestinationDistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Shipments_Districts_OriginDistrictId",
                        column: x => x.OriginDistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Shipments_LogisticsPartnerProfiles_LogisticsPartnerProfileId",
                        column: x => x.LogisticsPartnerProfileId,
                        principalTable: "LogisticsPartnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shipments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Shipments_PickupRequests_PickupRequestId",
                        column: x => x.PickupRequestId,
                        principalTable: "PickupRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Shipments_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NextAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryAttempts_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryAttempts_Users_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentTrackingEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LocationLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentTrackingEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentTrackingEvents_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ShipmentTrackingEvents_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShipmentTrackingEvents_Users_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAttempts_RecordedByUserId",
                table: "DeliveryAttempts",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAttempts_ShipmentId_AttemptNumber",
                table: "DeliveryAttempts",
                columns: new[] { "ShipmentId", "AttemptNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_CreatedByUserId",
                table: "Shipments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_DeliveryRouteId",
                table: "Shipments",
                column: "DeliveryRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_DestinationDistrictId",
                table: "Shipments",
                column: "DestinationDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_EstimatedDeliveryAt",
                table: "Shipments",
                column: "EstimatedDeliveryAt");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_LogisticsPartnerProfileId_Status",
                table: "Shipments",
                columns: new[] { "LogisticsPartnerProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_OrderId",
                table: "Shipments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_OriginDistrictId",
                table: "Shipments",
                column: "OriginDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_PickupRequestId",
                table: "Shipments",
                column: "PickupRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_TrackingNumber",
                table: "Shipments",
                column: "TrackingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentTrackingEvents_DistrictId",
                table: "ShipmentTrackingEvents",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentTrackingEvents_RecordedByUserId",
                table: "ShipmentTrackingEvents",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentTrackingEvents_ShipmentId_OccurredAt",
                table: "ShipmentTrackingEvents",
                columns: new[] { "ShipmentId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryAttempts");

            migrationBuilder.DropTable(
                name: "ShipmentTrackingEvents");

            migrationBuilder.DropTable(
                name: "Shipments");
        }
    }
}

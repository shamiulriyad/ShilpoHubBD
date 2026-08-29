using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLogisticsReturnHandlingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReturnRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    LogisticsPartnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationWarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReasonDetail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CustomerPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PickupContactName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    PickupPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    PickupAddressLine = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    PickupCity = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    PickupDistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    PickupPostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ScheduledPickupAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualPickupAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedCarrierLabel = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    AssignedDriverName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolutionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ResolutionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RefundAmount = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    RefundMethod = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    RefundReference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnRequests_Districts_PickupDistrictId",
                        column: x => x.PickupDistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReturnRequests_LogisticsPartnerProfiles_LogisticsPartnerPro~",
                        column: x => x.LogisticsPartnerProfileId,
                        principalTable: "LogisticsPartnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnRequests_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReturnRequests_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReturnRequests_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReturnRequests_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnRequests_Warehouses_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReturnEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnEvents_ReturnRequests_ReturnRequestId",
                        column: x => x.ReturnRequestId,
                        principalTable: "ReturnRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnEvents_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReturnInspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    InspectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OverallCondition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RecommendedResolution = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PhotosJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnInspections_ReturnRequests_ReturnRequestId",
                        column: x => x.ReturnRequestId,
                        principalTable: "ReturnRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnInspections_Users_InspectedByUserId",
                        column: x => x.InspectedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReturnItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sku = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    QuantityReceived = table.Column<int>(type: "integer", nullable: false),
                    RestockedQuantity = table.Column<int>(type: "integer", nullable: false),
                    Condition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Disposition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UnitRefundAmount = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReturnItems_ReturnRequests_ReturnRequestId",
                        column: x => x.ReturnRequestId,
                        principalTable: "ReturnRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnEvents_ActorUserId",
                table: "ReturnEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnEvents_ReturnRequestId_CreatedAt",
                table: "ReturnEvents",
                columns: new[] { "ReturnRequestId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnInspections_InspectedByUserId",
                table: "ReturnInspections",
                column: "InspectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnInspections_ReturnRequestId_InspectedAt",
                table: "ReturnInspections",
                columns: new[] { "ReturnRequestId", "InspectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnItems_ProductId",
                table: "ReturnItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnItems_ReturnRequestId",
                table: "ReturnItems",
                column: "ReturnRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_ApprovedByUserId",
                table: "ReturnRequests",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_CreatedByUserId",
                table: "ReturnRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_DestinationWarehouseId",
                table: "ReturnRequests",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_LogisticsPartnerProfileId_Status",
                table: "ReturnRequests",
                columns: new[] { "LogisticsPartnerProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_OrderId",
                table: "ReturnRequests",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_PickupDistrictId",
                table: "ReturnRequests",
                column: "PickupDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_ReferenceCode",
                table: "ReturnRequests",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_ShipmentId",
                table: "ReturnRequests",
                column: "ShipmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReturnEvents");

            migrationBuilder.DropTable(
                name: "ReturnInspections");

            migrationBuilder.DropTable(
                name: "ReturnItems");

            migrationBuilder.DropTable(
                name: "ReturnRequests");
        }
    }
}

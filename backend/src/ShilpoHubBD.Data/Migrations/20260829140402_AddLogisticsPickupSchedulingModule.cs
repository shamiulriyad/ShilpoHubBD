using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLogisticsPickupSchedulingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogisticsPartnerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactPersonName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BaseAddressLine = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    BaseCity = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    BaseDistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    BasePostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    FleetSize = table.Column<int>(type: "integer", nullable: false),
                    MaxDailyPickups = table.Column<int>(type: "integer", nullable: false),
                    MaxVehicleCapacityKg = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    OperatingDayStartHour = table.Column<int>(type: "integer", nullable: true),
                    OperatingDayEndHour = table.Column<int>(type: "integer", nullable: true),
                    OffersCashOnDelivery = table.Column<bool>(type: "boolean", nullable: false),
                    OffersColdChain = table.Column<bool>(type: "boolean", nullable: false),
                    OffersFragileHandling = table.Column<bool>(type: "boolean", nullable: false),
                    IsAcceptingRequests = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogisticsPartnerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogisticsPartnerProfiles_Districts_BaseDistrictId",
                        column: x => x.BaseDistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LogisticsPartnerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsPartnerProfiles_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsServiceAreas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogisticsPartnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Division = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    StandardDeliveryDays = table.Column<int>(type: "integer", nullable: false),
                    SupportsSameDay = table.Column<bool>(type: "boolean", nullable: false),
                    SurchargeAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogisticsServiceAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogisticsServiceAreas_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsServiceAreas_LogisticsPartnerProfiles_LogisticsPar~",
                        column: x => x.LogisticsPartnerProfileId,
                        principalTable: "LogisticsPartnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PickupRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    LogisticsPartnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginContactName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    OriginPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OriginAddressLine = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    OriginCity = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    OriginDistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginPostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OriginProducerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationContactName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    DestinationPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DestinationAddressLine = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    DestinationCity = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DestinationDistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledPickupAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PickupWindowEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualPickupAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PackageCount = table.Column<int>(type: "integer", nullable: false),
                    TotalWeightKg = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    DeclaredValue = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    RequiresColdChain = table.Column<bool>(type: "boolean", nullable: false),
                    IsFragile = table.Column<bool>(type: "boolean", nullable: false),
                    IsCashOnDelivery = table.Column<bool>(type: "boolean", nullable: false),
                    CodAmount = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    AssignedDriverName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    AssignedDriverPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    AssignedVehicleLabel = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SpecialInstructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickupRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickupRequests_Districts_DestinationDistrictId",
                        column: x => x.DestinationDistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PickupRequests_Districts_OriginDistrictId",
                        column: x => x.OriginDistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PickupRequests_LogisticsPartnerProfiles_LogisticsPartnerPro~",
                        column: x => x.LogisticsPartnerProfileId,
                        principalTable: "LogisticsPartnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PickupRequests_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PickupRequests_Users_OriginProducerUserId",
                        column: x => x.OriginProducerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PickupRequests_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PickupEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PickupRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickupEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickupEvents_PickupRequests_PickupRequestId",
                        column: x => x.PickupRequestId,
                        principalTable: "PickupRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PickupEvents_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PickupItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PickupRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    LengthCm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    WidthCm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    HeightCm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Reference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    IsFragile = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickupItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickupItems_PickupRequests_PickupRequestId",
                        column: x => x.PickupRequestId,
                        principalTable: "PickupRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPartnerProfiles_BaseDistrictId",
                table: "LogisticsPartnerProfiles",
                column: "BaseDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPartnerProfiles_UserId",
                table: "LogisticsPartnerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPartnerProfiles_VerificationStatus",
                table: "LogisticsPartnerProfiles",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsPartnerProfiles_VerifiedByUserId",
                table: "LogisticsPartnerProfiles",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsServiceAreas_DistrictId",
                table: "LogisticsServiceAreas",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsServiceAreas_LogisticsPartnerProfileId_DistrictId",
                table: "LogisticsServiceAreas",
                columns: new[] { "LogisticsPartnerProfileId", "DistrictId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickupEvents_ActorUserId",
                table: "PickupEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PickupEvents_PickupRequestId_CreatedAt",
                table: "PickupEvents",
                columns: new[] { "PickupRequestId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PickupItems_PickupRequestId",
                table: "PickupItems",
                column: "PickupRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PickupRequests_DestinationDistrictId",
                table: "PickupRequests",
                column: "DestinationDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_PickupRequests_LogisticsPartnerProfileId_Status",
                table: "PickupRequests",
                columns: new[] { "LogisticsPartnerProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PickupRequests_OrderId",
                table: "PickupRequests",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PickupRequests_OriginDistrictId",
                table: "PickupRequests",
                column: "OriginDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_PickupRequests_OriginProducerUserId",
                table: "PickupRequests",
                column: "OriginProducerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PickupRequests_ReferenceCode",
                table: "PickupRequests",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickupRequests_RequestedByUserId",
                table: "PickupRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PickupRequests_ScheduledPickupAt",
                table: "PickupRequests",
                column: "ScheduledPickupAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogisticsServiceAreas");

            migrationBuilder.DropTable(
                name: "PickupEvents");

            migrationBuilder.DropTable(
                name: "PickupItems");

            migrationBuilder.DropTable(
                name: "PickupRequests");

            migrationBuilder.DropTable(
                name: "LogisticsPartnerProfiles");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLogisticsAiModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryPredictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogisticsPartnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    PredictedDeliveryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PredictedTransitDays = table.Column<double>(type: "double precision", nullable: false),
                    OnTimeProbability = table.Column<double>(type: "double precision", nullable: false),
                    PredictedFailureProbability = table.Column<double>(type: "double precision", nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Confidence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FactorsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryPredictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryPredictions_LogisticsPartnerProfiles_LogisticsPartn~",
                        column: x => x.LogisticsPartnerProfileId,
                        principalTable: "LogisticsPartnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryPredictions_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryPredictions_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsDemandForecasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogisticsPartnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScopeLabel = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Metric = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HorizonDays = table.Column<int>(type: "integer", nullable: false),
                    Granularity = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Method = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Confidence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BaselineDailyAverage = table.Column<double>(type: "double precision", nullable: false),
                    PredictedTotal = table.Column<double>(type: "double precision", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssumptionsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogisticsDemandForecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogisticsDemandForecasts_LogisticsPartnerProfiles_Logistics~",
                        column: x => x.LogisticsPartnerProfileId,
                        principalTable: "LogisticsPartnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogisticsDemandForecasts_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouteOptimizationRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogisticsPartnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Method = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Objective = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OriginalDistanceKm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    ProposedDistanceKm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    DistanceSavingKm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    ProposedDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    Confidence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteOptimizationRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteOptimizationRuns_DeliveryRoutes_DeliveryRouteId",
                        column: x => x.DeliveryRouteId,
                        principalTable: "DeliveryRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteOptimizationRuns_LogisticsPartnerProfiles_LogisticsPar~",
                        column: x => x.LogisticsPartnerProfileId,
                        principalTable: "LogisticsPartnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteOptimizationRuns_Users_AppliedByUserId",
                        column: x => x.AppliedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RouteOptimizationRuns_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseAllocationRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogisticsPartnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Objective = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Sku = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    RequireColdChain = table.Column<bool>(type: "boolean", nullable: false),
                    DestinationDistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Method = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Confidence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RecommendedWarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecommendedWarehouseCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseAllocationRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseAllocationRecommendations_Districts_DestinationDis~",
                        column: x => x.DestinationDistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WarehouseAllocationRecommendations_LogisticsPartnerProfiles~",
                        column: x => x.LogisticsPartnerProfileId,
                        principalTable: "LogisticsPartnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseAllocationRecommendations_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WarehouseAllocationRecommendations_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseAllocationRecommendations_Warehouses_RecommendedWa~",
                        column: x => x.RecommendedWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsDemandForecastPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandForecastId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PredictedValue = table.Column<double>(type: "double precision", nullable: false),
                    LowerBound = table.Column<double>(type: "double precision", nullable: false),
                    UpperBound = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogisticsDemandForecastPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogisticsDemandForecastPoints_LogisticsDemandForecasts_Dema~",
                        column: x => x.DemandForecastId,
                        principalTable: "LogisticsDemandForecasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteOptimizationRunStops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteOptimizationRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteStopId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalSequence = table.Column<int>(type: "integer", nullable: false),
                    ProposedSequence = table.Column<int>(type: "integer", nullable: false),
                    DistanceFromPreviousKm = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Label = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteOptimizationRunStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteOptimizationRunStops_RouteOptimizationRuns_RouteOptimi~",
                        column: x => x.RouteOptimizationRunId,
                        principalTable: "RouteOptimizationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseAllocationOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseAllocationRecommendationId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    WarehouseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    ProjectedUtilizationPercent = table.Column<double>(type: "double precision", nullable: false),
                    SameDistrictAsDestination = table.Column<bool>(type: "boolean", nullable: false),
                    Rationale = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseAllocationOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseAllocationOptions_WarehouseAllocationRecommendatio~",
                        column: x => x.WarehouseAllocationRecommendationId,
                        principalTable: "WarehouseAllocationRecommendations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPredictions_GeneratedByUserId",
                table: "DeliveryPredictions",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPredictions_LogisticsPartnerProfileId_CreatedAt",
                table: "DeliveryPredictions",
                columns: new[] { "LogisticsPartnerProfileId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPredictions_ShipmentId",
                table: "DeliveryPredictions",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsDemandForecastPoints_DemandForecastId_PeriodDate",
                table: "LogisticsDemandForecastPoints",
                columns: new[] { "DemandForecastId", "PeriodDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsDemandForecasts_GeneratedByUserId",
                table: "LogisticsDemandForecasts",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsDemandForecasts_LogisticsPartnerProfileId_CreatedAt",
                table: "LogisticsDemandForecasts",
                columns: new[] { "LogisticsPartnerProfileId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsDemandForecasts_Scope_ScopeId",
                table: "LogisticsDemandForecasts",
                columns: new[] { "Scope", "ScopeId" });

            migrationBuilder.CreateIndex(
                name: "IX_RouteOptimizationRuns_AppliedByUserId",
                table: "RouteOptimizationRuns",
                column: "AppliedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteOptimizationRuns_DeliveryRouteId_Status",
                table: "RouteOptimizationRuns",
                columns: new[] { "DeliveryRouteId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RouteOptimizationRuns_GeneratedByUserId",
                table: "RouteOptimizationRuns",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteOptimizationRuns_LogisticsPartnerProfileId_CreatedAt",
                table: "RouteOptimizationRuns",
                columns: new[] { "LogisticsPartnerProfileId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RouteOptimizationRunStops_RouteOptimizationRunId_ProposedSe~",
                table: "RouteOptimizationRunStops",
                columns: new[] { "RouteOptimizationRunId", "ProposedSequence" });

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseAllocationOptions_WarehouseAllocationRecommendatio~",
                table: "WarehouseAllocationOptions",
                columns: new[] { "WarehouseAllocationRecommendationId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseAllocationRecommendations_DestinationDistrictId",
                table: "WarehouseAllocationRecommendations",
                column: "DestinationDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseAllocationRecommendations_GeneratedByUserId",
                table: "WarehouseAllocationRecommendations",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseAllocationRecommendations_LogisticsPartnerProfileI~",
                table: "WarehouseAllocationRecommendations",
                columns: new[] { "LogisticsPartnerProfileId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseAllocationRecommendations_RecommendedWarehouseId",
                table: "WarehouseAllocationRecommendations",
                column: "RecommendedWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseAllocationRecommendations_ShipmentId",
                table: "WarehouseAllocationRecommendations",
                column: "ShipmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryPredictions");

            migrationBuilder.DropTable(
                name: "LogisticsDemandForecastPoints");

            migrationBuilder.DropTable(
                name: "RouteOptimizationRunStops");

            migrationBuilder.DropTable(
                name: "WarehouseAllocationOptions");

            migrationBuilder.DropTable(
                name: "LogisticsDemandForecasts");

            migrationBuilder.DropTable(
                name: "RouteOptimizationRuns");

            migrationBuilder.DropTable(
                name: "WarehouseAllocationRecommendations");
        }
    }
}

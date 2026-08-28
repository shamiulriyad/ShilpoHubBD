using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGovernancePolicySimulatorModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PolicySimulations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    SimulationType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScopeLabel = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HorizonMonths = table.Column<int>(type: "integer", nullable: false),
                    InputsJson = table.Column<string>(type: "text", nullable: false),
                    AssumptionsJson = table.Column<string>(type: "text", nullable: true),
                    Method = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Confidence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BaselineProducers = table.Column<int>(type: "integer", nullable: false),
                    BaselineActiveProducers = table.Column<int>(type: "integer", nullable: false),
                    BaselineEmployment = table.Column<int>(type: "integer", nullable: false),
                    BaselineExportValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BaselineTourismRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BaselineEconomyValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicySimulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicySimulations_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PolicySimulationProjections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicySimulationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Metric = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BaselineValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProjectedValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DeltaValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DeltaPercent = table.Column<double>(type: "double precision", nullable: false),
                    HorizonMonths = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Detail = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicySimulationProjections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicySimulationProjections_PolicySimulations_PolicySimulat~",
                        column: x => x.PolicySimulationId,
                        principalTable: "PolicySimulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PolicySimulationRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicySimulationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Detail = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicySimulationRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicySimulationRecommendations_PolicySimulations_PolicySim~",
                        column: x => x.PolicySimulationId,
                        principalTable: "PolicySimulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolicySimulationProjections_PolicySimulationId_DisplayOrder",
                table: "PolicySimulationProjections",
                columns: new[] { "PolicySimulationId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicySimulationRecommendations_PolicySimulationId_DisplayO~",
                table: "PolicySimulationRecommendations",
                columns: new[] { "PolicySimulationId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicySimulations_CreatedAt",
                table: "PolicySimulations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PolicySimulations_GeneratedByUserId",
                table: "PolicySimulations",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicySimulations_Scope_ScopeId",
                table: "PolicySimulations",
                columns: new[] { "Scope", "ScopeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicySimulations_SimulationType",
                table: "PolicySimulations",
                column: "SimulationType");

            migrationBuilder.CreateIndex(
                name: "IX_PolicySimulations_Status",
                table: "PolicySimulations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolicySimulationProjections");

            migrationBuilder.DropTable(
                name: "PolicySimulationRecommendations");

            migrationBuilder.DropTable(
                name: "PolicySimulations");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSustainabilityModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SustainabilityProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    EcoScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    BadgeLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalCarbonSavingsKg = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastCalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SustainabilityProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SustainabilityProfiles_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SustainableMaterialCertifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SustainabilityProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CertifyingBody = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CertificateReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SustainableMaterialCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SustainableMaterialCertifications_SustainabilityProfiles_Su~",
                        column: x => x.SustainabilityProfileId,
                        principalTable: "SustainabilityProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SustainableMaterialRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SustainabilityProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaterialName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    QuantityUsed = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsRecycled = table.Column<bool>(type: "boolean", nullable: false),
                    IsRenewable = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocallySourced = table.Column<bool>(type: "boolean", nullable: false),
                    IsBiodegradable = table.Column<bool>(type: "boolean", nullable: false),
                    CarbonSavingsPerUnitKg = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SustainableMaterialRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SustainableMaterialRecords_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SustainableMaterialRecords_SustainabilityProfiles_Sustainab~",
                        column: x => x.SustainabilityProfileId,
                        principalTable: "SustainabilityProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SustainabilityProfiles_ProducerId",
                table: "SustainabilityProfiles",
                column: "ProducerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SustainableMaterialCertifications_SustainabilityProfileId",
                table: "SustainableMaterialCertifications",
                column: "SustainabilityProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SustainableMaterialRecords_ProductId",
                table: "SustainableMaterialRecords",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SustainableMaterialRecords_SustainabilityProfileId",
                table: "SustainableMaterialRecords",
                column: "SustainabilityProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SustainableMaterialCertifications");

            migrationBuilder.DropTable(
                name: "SustainableMaterialRecords");

            migrationBuilder.DropTable(
                name: "SustainabilityProfiles");
        }
    }
}

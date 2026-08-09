using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturingPartnershipModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManufacturingPartnerships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProductRequirements = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ManufacturingSpecifications = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    TargetUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TimelineStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimelineEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProducerResponseNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingPartnerships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturingPartnerships_Users_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManufacturingPartnerships_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturingMilestones_ManufacturingPartnerships_Partnersh~",
                        column: x => x.PartnershipId,
                        principalTable: "ManufacturingPartnerships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartnershipStatusEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnershipStatusEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnershipStatusEvents_ManufacturingPartnerships_Partnersh~",
                        column: x => x.PartnershipId,
                        principalTable: "ManufacturingPartnerships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingMilestones_PartnershipId",
                table: "ManufacturingMilestones",
                column: "PartnershipId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingPartnerships_BusinessPartnerId",
                table: "ManufacturingPartnerships",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingPartnerships_ProducerId",
                table: "ManufacturingPartnerships",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingPartnerships_ReferenceNumber",
                table: "ManufacturingPartnerships",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartnershipStatusEvents_PartnershipId",
                table: "PartnershipStatusEvents",
                column: "PartnershipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManufacturingMilestones");

            migrationBuilder.DropTable(
                name: "PartnershipStatusEvents");

            migrationBuilder.DropTable(
                name: "ManufacturingPartnerships");
        }
    }
}

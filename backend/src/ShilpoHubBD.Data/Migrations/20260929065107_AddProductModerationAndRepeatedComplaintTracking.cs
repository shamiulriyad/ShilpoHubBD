using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductModerationAndRepeatedComplaintTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProducerModerationWarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplaintType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SimilarComplaintCount = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerModerationWarnings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductModerationStates",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    NegativeComplaintCount = table.Column<int>(type: "integer", nullable: false),
                    SimilarComplaintCount = table.Column<int>(type: "integer", nullable: false),
                    HighSeverityComplaintCount = table.Column<int>(type: "integer", nullable: false),
                    ProducerWarningCount = table.Column<int>(type: "integer", nullable: false),
                    RiskState = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastReviewId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastEvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductModerationStates", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_ProductModerationStates_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductModerationEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ComplaintType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    IsAiGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    SimilarReviewIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductModerationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductModerationEvents_ProductModerationStates_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ProductModerationStates",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProducerModerationWarnings_ProducerId",
                table: "ProducerModerationWarnings",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerModerationWarnings_ProductId",
                table: "ProducerModerationWarnings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModerationEvents_ProductId_CreatedAt",
                table: "ProductModerationEvents",
                columns: new[] { "ProductId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductModerationStates_RiskState",
                table: "ProductModerationStates",
                column: "RiskState");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProducerModerationWarnings");

            migrationBuilder.DropTable(
                name: "ProductModerationEvents");

            migrationBuilder.DropTable(
                name: "ProductModerationStates");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcurementRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Budget = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DeliveryDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    QuotationRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuotationResponseId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_QuotationRequests_QuotationRequestId",
                        column: x => x.QuotationRequestId,
                        principalTable: "QuotationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_QuotationResponses_QuotationResponseId",
                        column: x => x.QuotationResponseId,
                        principalTable: "QuotationResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_Users_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcurementRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Specifications = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementItems_ProcurementRequests_ProcurementRequestId",
                        column: x => x.ProcurementRequestId,
                        principalTable: "ProcurementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcurementItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementStatusEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcurementRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementStatusEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementStatusEvents_ProcurementRequests_ProcurementRequ~",
                        column: x => x.ProcurementRequestId,
                        principalTable: "ProcurementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementItems_ProcurementRequestId",
                table: "ProcurementItems",
                column: "ProcurementRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementItems_ProductId",
                table: "ProcurementItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_ApprovedByUserId",
                table: "ProcurementRequests",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_BusinessPartnerId",
                table: "ProcurementRequests",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_OrderId",
                table: "ProcurementRequests",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_ProducerId",
                table: "ProcurementRequests",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_QuotationRequestId",
                table: "ProcurementRequests",
                column: "QuotationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_QuotationResponseId",
                table: "ProcurementRequests",
                column: "QuotationResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_ReferenceNumber",
                table: "ProcurementRequests",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementStatusEvents_ProcurementRequestId",
                table: "ProcurementStatusEvents",
                column: "ProcurementRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcurementItems");

            migrationBuilder.DropTable(
                name: "ProcurementStatusEvents");

            migrationBuilder.DropTable(
                name: "ProcurementRequests");
        }
    }
}

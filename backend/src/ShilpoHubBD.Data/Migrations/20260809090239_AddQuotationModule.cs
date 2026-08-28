using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuotationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Requirements = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RequiredDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationRequests_Users_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationRequestItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    TargetPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Specifications = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationRequestItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QuotationRequestItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QuotationRequestItems_QuotationRequests_QuotationRequestId",
                        column: x => x.QuotationRequestId,
                        principalTable: "QuotationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuotationRequestProducers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationRequestProducers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationRequestProducers_QuotationRequests_QuotationReques~",
                        column: x => x.QuotationRequestId,
                        principalTable: "QuotationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuotationRequestProducers_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationStatusEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationStatusEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationStatusEvents_QuotationRequests_QuotationRequestId",
                        column: x => x.QuotationRequestId,
                        principalTable: "QuotationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuotationResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationRequestProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationResponses_QuotationRequestProducers_QuotationReque~",
                        column: x => x.QuotationRequestProducerId,
                        principalTable: "QuotationRequestProducers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuotationResponseItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationResponseId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationRequestItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotedUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    QuotedQuantity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationResponseItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationResponseItems_QuotationRequestItems_QuotationReque~",
                        column: x => x.QuotationRequestItemId,
                        principalTable: "QuotationRequestItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationResponseItems_QuotationResponses_QuotationResponse~",
                        column: x => x.QuotationResponseId,
                        principalTable: "QuotationResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequestItems_CategoryId",
                table: "QuotationRequestItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequestItems_ProductId",
                table: "QuotationRequestItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequestItems_QuotationRequestId",
                table: "QuotationRequestItems",
                column: "QuotationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequestProducers_ProducerId",
                table: "QuotationRequestProducers",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequestProducers_QuotationRequestId_ProducerId",
                table: "QuotationRequestProducers",
                columns: new[] { "QuotationRequestId", "ProducerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequests_BusinessPartnerId",
                table: "QuotationRequests",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationRequests_ReferenceNumber",
                table: "QuotationRequests",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationResponseItems_QuotationRequestItemId",
                table: "QuotationResponseItems",
                column: "QuotationRequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationResponseItems_QuotationResponseId",
                table: "QuotationResponseItems",
                column: "QuotationResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationResponses_QuotationRequestProducerId",
                table: "QuotationResponses",
                column: "QuotationRequestProducerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationStatusEvents_QuotationRequestId",
                table: "QuotationStatusEvents",
                column: "QuotationRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationResponseItems");

            migrationBuilder.DropTable(
                name: "QuotationStatusEvents");

            migrationBuilder.DropTable(
                name: "QuotationRequestItems");

            migrationBuilder.DropTable(
                name: "QuotationResponses");

            migrationBuilder.DropTable(
                name: "QuotationRequestProducers");

            migrationBuilder.DropTable(
                name: "QuotationRequests");
        }
    }
}

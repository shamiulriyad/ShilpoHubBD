using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveShoppingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiveEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScheduledStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveEvents_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LiveEvents_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LiveEventComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LiveEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveEventComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveEventComments_LiveEvents_LiveEventId",
                        column: x => x.LiveEventId,
                        principalTable: "LiveEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveEventComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LiveEventPurchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LiveEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveEventPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveEventPurchases_LiveEvents_LiveEventId",
                        column: x => x.LiveEventId,
                        principalTable: "LiveEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveEventPurchases_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LiveEventPurchases_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LiveEventReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LiveEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveEventReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveEventReactions_LiveEvents_LiveEventId",
                        column: x => x.LiveEventId,
                        principalTable: "LiveEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveEventReactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveEventComments_LiveEventId",
                table: "LiveEventComments",
                column: "LiveEventId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEventComments_UserId",
                table: "LiveEventComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEventPurchases_LiveEventId",
                table: "LiveEventPurchases",
                column: "LiveEventId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEventPurchases_ProductId",
                table: "LiveEventPurchases",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEventPurchases_UserId",
                table: "LiveEventPurchases",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEventReactions_LiveEventId",
                table: "LiveEventReactions",
                column: "LiveEventId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEventReactions_UserId",
                table: "LiveEventReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_ProducerId",
                table: "LiveEvents",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_ProductId",
                table: "LiveEvents",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_Status",
                table: "LiveEvents",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveEventComments");

            migrationBuilder.DropTable(
                name: "LiveEventPurchases");

            migrationBuilder.DropTable(
                name: "LiveEventReactions");

            migrationBuilder.DropTable(
                name: "LiveEvents");
        }
    }
}

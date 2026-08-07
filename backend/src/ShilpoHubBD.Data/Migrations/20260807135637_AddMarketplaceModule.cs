using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketplaceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Division = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DiscountPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    SalesCount = table.Column<int>(type: "integer", nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Districts",
                columns: new[] { "Id", "DisplayOrder", "Division", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), 1, "Barishal", true, "Barguna" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), 2, "Barishal", true, "Barishal" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), 3, "Barishal", true, "Bhola" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), 4, "Barishal", true, "Jhalokati" },
                    { new Guid("20000000-0000-0000-0000-000000000005"), 5, "Barishal", true, "Patuakhali" },
                    { new Guid("20000000-0000-0000-0000-000000000006"), 6, "Barishal", true, "Pirojpur" },
                    { new Guid("20000000-0000-0000-0000-000000000007"), 7, "Chattogram", true, "Bandarban" },
                    { new Guid("20000000-0000-0000-0000-000000000008"), 8, "Chattogram", true, "Brahmanbaria" },
                    { new Guid("20000000-0000-0000-0000-000000000009"), 9, "Chattogram", true, "Chandpur" },
                    { new Guid("20000000-0000-0000-0000-000000000010"), 10, "Chattogram", true, "Chattogram" },
                    { new Guid("20000000-0000-0000-0000-000000000011"), 11, "Chattogram", true, "Cumilla" },
                    { new Guid("20000000-0000-0000-0000-000000000012"), 12, "Chattogram", true, "Cox's Bazar" },
                    { new Guid("20000000-0000-0000-0000-000000000013"), 13, "Chattogram", true, "Feni" },
                    { new Guid("20000000-0000-0000-0000-000000000014"), 14, "Chattogram", true, "Khagrachhari" },
                    { new Guid("20000000-0000-0000-0000-000000000015"), 15, "Chattogram", true, "Lakshmipur" },
                    { new Guid("20000000-0000-0000-0000-000000000016"), 16, "Chattogram", true, "Noakhali" },
                    { new Guid("20000000-0000-0000-0000-000000000017"), 17, "Chattogram", true, "Rangamati" },
                    { new Guid("20000000-0000-0000-0000-000000000018"), 18, "Dhaka", true, "Dhaka" },
                    { new Guid("20000000-0000-0000-0000-000000000019"), 19, "Dhaka", true, "Faridpur" },
                    { new Guid("20000000-0000-0000-0000-000000000020"), 20, "Dhaka", true, "Gazipur" },
                    { new Guid("20000000-0000-0000-0000-000000000021"), 21, "Dhaka", true, "Gopalganj" },
                    { new Guid("20000000-0000-0000-0000-000000000022"), 22, "Dhaka", true, "Kishoreganj" },
                    { new Guid("20000000-0000-0000-0000-000000000023"), 23, "Dhaka", true, "Madaripur" },
                    { new Guid("20000000-0000-0000-0000-000000000024"), 24, "Dhaka", true, "Manikganj" },
                    { new Guid("20000000-0000-0000-0000-000000000025"), 25, "Dhaka", true, "Munshiganj" },
                    { new Guid("20000000-0000-0000-0000-000000000026"), 26, "Dhaka", true, "Narayanganj" },
                    { new Guid("20000000-0000-0000-0000-000000000027"), 27, "Dhaka", true, "Narsingdi" },
                    { new Guid("20000000-0000-0000-0000-000000000028"), 28, "Dhaka", true, "Rajbari" },
                    { new Guid("20000000-0000-0000-0000-000000000029"), 29, "Dhaka", true, "Shariatpur" },
                    { new Guid("20000000-0000-0000-0000-000000000030"), 30, "Dhaka", true, "Tangail" },
                    { new Guid("20000000-0000-0000-0000-000000000031"), 31, "Khulna", true, "Bagerhat" },
                    { new Guid("20000000-0000-0000-0000-000000000032"), 32, "Khulna", true, "Chuadanga" },
                    { new Guid("20000000-0000-0000-0000-000000000033"), 33, "Khulna", true, "Jashore" },
                    { new Guid("20000000-0000-0000-0000-000000000034"), 34, "Khulna", true, "Jhenaidah" },
                    { new Guid("20000000-0000-0000-0000-000000000035"), 35, "Khulna", true, "Khulna" },
                    { new Guid("20000000-0000-0000-0000-000000000036"), 36, "Khulna", true, "Kushtia" },
                    { new Guid("20000000-0000-0000-0000-000000000037"), 37, "Khulna", true, "Magura" },
                    { new Guid("20000000-0000-0000-0000-000000000038"), 38, "Khulna", true, "Meherpur" },
                    { new Guid("20000000-0000-0000-0000-000000000039"), 39, "Khulna", true, "Narail" },
                    { new Guid("20000000-0000-0000-0000-000000000040"), 40, "Khulna", true, "Satkhira" },
                    { new Guid("20000000-0000-0000-0000-000000000041"), 41, "Mymensingh", true, "Jamalpur" },
                    { new Guid("20000000-0000-0000-0000-000000000042"), 42, "Mymensingh", true, "Mymensingh" },
                    { new Guid("20000000-0000-0000-0000-000000000043"), 43, "Mymensingh", true, "Netrokona" },
                    { new Guid("20000000-0000-0000-0000-000000000044"), 44, "Mymensingh", true, "Sherpur" },
                    { new Guid("20000000-0000-0000-0000-000000000045"), 45, "Rajshahi", true, "Bogura" },
                    { new Guid("20000000-0000-0000-0000-000000000046"), 46, "Rajshahi", true, "Chapainawabganj" },
                    { new Guid("20000000-0000-0000-0000-000000000047"), 47, "Rajshahi", true, "Joypurhat" },
                    { new Guid("20000000-0000-0000-0000-000000000048"), 48, "Rajshahi", true, "Naogaon" },
                    { new Guid("20000000-0000-0000-0000-000000000049"), 49, "Rajshahi", true, "Natore" },
                    { new Guid("20000000-0000-0000-0000-000000000050"), 50, "Rajshahi", true, "Pabna" },
                    { new Guid("20000000-0000-0000-0000-000000000051"), 51, "Rajshahi", true, "Rajshahi" },
                    { new Guid("20000000-0000-0000-0000-000000000052"), 52, "Rajshahi", true, "Sirajganj" },
                    { new Guid("20000000-0000-0000-0000-000000000053"), 53, "Rangpur", true, "Dinajpur" },
                    { new Guid("20000000-0000-0000-0000-000000000054"), 54, "Rangpur", true, "Gaibandha" },
                    { new Guid("20000000-0000-0000-0000-000000000055"), 55, "Rangpur", true, "Kurigram" },
                    { new Guid("20000000-0000-0000-0000-000000000056"), 56, "Rangpur", true, "Lalmonirhat" },
                    { new Guid("20000000-0000-0000-0000-000000000057"), 57, "Rangpur", true, "Nilphamari" },
                    { new Guid("20000000-0000-0000-0000-000000000058"), 58, "Rangpur", true, "Panchagarh" },
                    { new Guid("20000000-0000-0000-0000-000000000059"), 59, "Rangpur", true, "Rangpur" },
                    { new Guid("20000000-0000-0000-0000-000000000060"), 60, "Rangpur", true, "Thakurgaon" },
                    { new Guid("20000000-0000-0000-0000-000000000061"), 61, "Sylhet", true, "Habiganj" },
                    { new Guid("20000000-0000-0000-0000-000000000062"), 62, "Sylhet", true, "Moulvibazar" },
                    { new Guid("20000000-0000-0000-0000-000000000063"), 63, "Sylhet", true, "Sunamganj" },
                    { new Guid("20000000-0000-0000-0000-000000000064"), 64, "Sylhet", true, "Sylhet" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_Name",
                table: "Districts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DistrictId",
                table: "Products",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_CategoryId",
                table: "Products",
                columns: new[] { "IsActive", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_DistrictId",
                table: "Products",
                columns: new[] { "IsActive", "DistrictId" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_IsFeatured",
                table: "Products",
                columns: new[] { "IsActive", "IsFeatured" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProducerId",
                table: "Products",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Districts");
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSearchFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Trigram index on Products.Name below needs this extension (safe if it already exists).
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductTypeId",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BayesianRating",
                table: "Products",
                type: "numeric(3,2)",
                nullable: false,
                computedColumnSql: "ROUND((20.0 + \"ReviewCount\" * \"AverageRating\") / (5 + \"ReviewCount\"), 2)",
                stored: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EffectivePrice",
                table: "Products",
                type: "numeric(10,2)",
                nullable: false,
                computedColumnSql: "COALESCE(\"DiscountPrice\", \"Price\")",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Products",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('english', coalesce(\"Name\", '') || ' ' || coalesce(\"Description\", '') || ' ' || coalesce(\"Story\", ''))",
                stored: true);

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NameBn = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductAttributes",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    Keywords = table.Column<List<string>>(type: "text[]", nullable: false),
                    Occasions = table.Column<List<string>>(type: "text[]", nullable: false),
                    Colors = table.Column<List<string>>(type: "text[]", nullable: false),
                    CraftTechnique = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProductionMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DimensionsText = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    LengthCm = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    WidthCm = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    HeightCm = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    WeightGrams = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    MadeToOrder = table.Column<bool>(type: "boolean", nullable: false),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    CareInstructions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributes", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_ProductAttributes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductAttributeSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAttributeSuggestions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductAttributeSuggestions_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProductIndexStates",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    SearchTextHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    EmbeddingModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IndexedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductIndexStates", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NameBn = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductMaterials",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMaterials", x => new { x.ProductId, x.MaterialId });
                    table.ForeignKey(
                        name: "FK_ProductMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductMaterials_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name_trgm",
                table: "Products",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_SearchVector",
                table: "Products",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Storefront_Category_Price",
                table: "Products",
                columns: new[] { "CategoryId", "EffectivePrice" },
                filter: "\"IsActive\" AND \"ApprovalStatus\" = 'Approved'");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Storefront_District_Price",
                table: "Products",
                columns: new[] { "DistrictId", "EffectivePrice" },
                filter: "\"IsActive\" AND \"ApprovalStatus\" = 'Approved'");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Storefront_InStock_Price",
                table: "Products",
                column: "EffectivePrice",
                filter: "\"IsActive\" AND \"ApprovalStatus\" = 'Approved' AND \"Stock\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Storefront_Newest",
                table: "Products",
                column: "CreatedAt",
                descending: new bool[0],
                filter: "\"IsActive\" AND \"ApprovalStatus\" = 'Approved'");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Storefront_Rating",
                table: "Products",
                column: "BayesianRating",
                descending: new bool[0],
                filter: "\"IsActive\" AND \"ApprovalStatus\" = 'Approved'");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Storefront_Sales",
                table: "Products",
                column: "SalesCount",
                descending: new bool[0],
                filter: "\"IsActive\" AND \"ApprovalStatus\" = 'Approved'");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Storefront_Type_Price",
                table: "Products",
                columns: new[] { "ProductTypeId", "EffectivePrice" },
                filter: "\"IsActive\" AND \"ApprovalStatus\" = 'Approved'");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_IsActive_DisplayOrder",
                table: "Materials",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Slug",
                table: "Materials",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_Colors",
                table: "ProductAttributes",
                column: "Colors")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_Keywords",
                table: "ProductAttributes",
                column: "Keywords")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_Occasions",
                table: "ProductAttributes",
                column: "Occasions")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_Tags",
                table: "ProductAttributes",
                column: "Tags")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeSuggestions_ProductId_Status",
                table: "ProductAttributeSuggestions",
                columns: new[] { "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeSuggestions_ReviewedByUserId",
                table: "ProductAttributeSuggestions",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductIndexStates_Pending",
                table: "ProductIndexStates",
                columns: new[] { "Status", "UpdatedAt" },
                filter: "\"Status\" <> 'Synced'");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMaterials_MaterialId_ProductId",
                table: "ProductMaterials",
                columns: new[] { "MaterialId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_IsActive_DisplayOrder",
                table: "ProductTypes",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_Slug",
                table: "ProductTypes",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductTypes_ProductTypeId",
                table: "Products",
                column: "ProductTypeId",
                principalTable: "ProductTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductTypes_ProductTypeId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductAttributes");

            migrationBuilder.DropTable(
                name: "ProductAttributeSuggestions");

            migrationBuilder.DropTable(
                name: "ProductIndexStates");

            migrationBuilder.DropTable(
                name: "ProductMaterials");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Products_Name_trgm",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SearchVector",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Storefront_Category_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Storefront_District_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Storefront_InStock_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Storefront_Newest",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Storefront_Rating",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Storefront_Sales",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Storefront_Type_Price",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BayesianRating",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "EffectivePrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductTypeId",
                table: "Products");
        }
    }
}

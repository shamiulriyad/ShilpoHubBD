using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteContentAndCraftHeritage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CraftHeritageEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Aliases = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Region = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GiName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Unesco = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    History = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Materials = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Process = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Products = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Story = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Visit = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Sources = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CraftHeritageEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteContentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Group = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LinkUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Extra = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteContentItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CraftHeritageEntries_DisplayOrder",
                table: "CraftHeritageEntries",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CraftHeritageEntries_IsActive",
                table: "CraftHeritageEntries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CraftHeritageEntries_Slug",
                table: "CraftHeritageEntries",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteContentItems_Group_DisplayOrder",
                table: "SiteContentItems",
                columns: new[] { "Group", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteContentItems_IsActive",
                table: "SiteContentItems",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CraftHeritageEntries");

            migrationBuilder.DropTable(
                name: "SiteContentItems");
        }
    }
}

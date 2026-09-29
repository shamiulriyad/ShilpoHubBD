using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArtisanSupportCaseExpectedOutcomeAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpectedOutcome",
                table: "ArtisanSupportCases",
                type: "character varying(6000)",
                maxLength: 6000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ArtisanSupportCases",
                type: "character varying(6000)",
                maxLength: 6000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedOutcome",
                table: "ArtisanSupportCases");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ArtisanSupportCases");
        }
    }
}

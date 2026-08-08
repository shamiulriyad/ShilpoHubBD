using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeritageIdentityModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProducerHeritageIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageIdNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrimaryCraft = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: false),
                    WorkshopName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WorkshopDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    WorkshopAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    EstablishedYear = table.Column<int>(type: "integer", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LegacyScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerHeritageIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerHeritageIdentities_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProducerHeritageIdentities_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProducerHeritageIdentities_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyHeritageMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerHeritageIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Relation = table.Column<string>(type: "text", nullable: false),
                    Generation = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: true),
                    ActiveYearsRange = table.Column<string>(type: "text", nullable: true),
                    Story = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyHeritageMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyHeritageMembers_ProducerHeritageIdentities_ProducerHe~",
                        column: x => x.ProducerHeritageIdentityId,
                        principalTable: "ProducerHeritageIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeritageAwards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerHeritageIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    IssuingOrganization = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageAwards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageAwards_ProducerHeritageIdentities_ProducerHeritageI~",
                        column: x => x.ProducerHeritageIdentityId,
                        principalTable: "ProducerHeritageIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeritageCertifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerHeritageIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IssuingBody = table.Column<string>(type: "text", nullable: false),
                    IssuedYear = table.Column<int>(type: "integer", nullable: false),
                    ExpiryYear = table.Column<int>(type: "integer", nullable: true),
                    CertificateNumber = table.Column<string>(type: "text", nullable: true),
                    CertificateUrl = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageCertifications_ProducerHeritageIdentities_ProducerH~",
                        column: x => x.ProducerHeritageIdentityId,
                        principalTable: "ProducerHeritageIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillTimelineEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerHeritageIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillTimelineEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillTimelineEntries_ProducerHeritageIdentities_ProducerHer~",
                        column: x => x.ProducerHeritageIdentityId,
                        principalTable: "ProducerHeritageIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoryArchiveEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerHeritageIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryArchiveEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoryArchiveEntries_ProducerHeritageIdentities_ProducerHeri~",
                        column: x => x.ProducerHeritageIdentityId,
                        principalTable: "ProducerHeritageIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyHeritageMembers_ProducerHeritageIdentityId",
                table: "FamilyHeritageMembers",
                column: "ProducerHeritageIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageAwards_ProducerHeritageIdentityId",
                table: "HeritageAwards",
                column: "ProducerHeritageIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageCertifications_ProducerHeritageIdentityId",
                table: "HeritageCertifications",
                column: "ProducerHeritageIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerHeritageIdentities_DistrictId",
                table: "ProducerHeritageIdentities",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerHeritageIdentities_HeritageIdNumber",
                table: "ProducerHeritageIdentities",
                column: "HeritageIdNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProducerHeritageIdentities_ProducerId",
                table: "ProducerHeritageIdentities",
                column: "ProducerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProducerHeritageIdentities_VerifiedByUserId",
                table: "ProducerHeritageIdentities",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillTimelineEntries_ProducerHeritageIdentityId",
                table: "SkillTimelineEntries",
                column: "ProducerHeritageIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryArchiveEntries_ProducerHeritageIdentityId",
                table: "StoryArchiveEntries",
                column: "ProducerHeritageIdentityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyHeritageMembers");

            migrationBuilder.DropTable(
                name: "HeritageAwards");

            migrationBuilder.DropTable(
                name: "HeritageCertifications");

            migrationBuilder.DropTable(
                name: "SkillTimelineEntries");

            migrationBuilder.DropTable(
                name: "StoryArchiveEntries");

            migrationBuilder.DropTable(
                name: "ProducerHeritageIdentities");
        }
    }
}

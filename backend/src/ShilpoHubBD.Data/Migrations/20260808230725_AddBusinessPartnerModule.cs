using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessPartnerModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessPartnerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TaxIdentificationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    YearEstablished = table.Column<int>(type: "integer", nullable: true),
                    Industry = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BusinessSize = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmployeeCount = table.Column<int>(type: "integer", nullable: true),
                    Website = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CompanyDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactPersonName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ContactPersonDesignation = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MinimumOrderQuantity = table.Column<int>(type: "integer", nullable: true),
                    MaxBudgetPerOrder = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PreferredOrderFrequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PreferredPaymentTerms = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VerificationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessPartnerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessPartnerProfiles_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessPartnerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessPartnerProfiles_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BusinessDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessDocuments_BusinessPartnerProfiles_BusinessPartnerPr~",
                        column: x => x.BusinessPartnerProfileId,
                        principalTable: "BusinessPartnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessPartnerPreferredCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessPartnerPreferredCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessPartnerPreferredCategories_BusinessPartnerProfiles_~",
                        column: x => x.BusinessPartnerProfileId,
                        principalTable: "BusinessPartnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessPartnerPreferredCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessDocuments_BusinessPartnerProfileId",
                table: "BusinessDocuments",
                column: "BusinessPartnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPartnerPreferredCategories_BusinessPartnerProfileId~",
                table: "BusinessPartnerPreferredCategories",
                columns: new[] { "BusinessPartnerProfileId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPartnerPreferredCategories_CategoryId",
                table: "BusinessPartnerPreferredCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPartnerProfiles_DistrictId",
                table: "BusinessPartnerProfiles",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPartnerProfiles_UserId",
                table: "BusinessPartnerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPartnerProfiles_VerifiedByUserId",
                table: "BusinessPartnerProfiles",
                column: "VerifiedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessDocuments");

            migrationBuilder.DropTable(
                name: "BusinessPartnerPreferredCategories");

            migrationBuilder.DropTable(
                name: "BusinessPartnerProfiles");
        }
    }
}

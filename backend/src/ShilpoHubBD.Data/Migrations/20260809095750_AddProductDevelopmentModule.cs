using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDevelopmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductDevelopmentProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BusinessRequirements = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ProductSpecifications = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDevelopmentProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDevelopmentProjects_Products_FinalProductId",
                        column: x => x.FinalProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductDevelopmentProjects_Users_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductDevelopmentProjects_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductDevelopmentComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDevelopmentComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDevelopmentComments_ProductDevelopmentProjects_Proje~",
                        column: x => x.ProjectId,
                        principalTable: "ProductDevelopmentProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductDevelopmentComments_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductDevelopmentMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDevelopmentMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDevelopmentMilestones_ProductDevelopmentProjects_Pro~",
                        column: x => x.ProjectId,
                        principalTable: "ProductDevelopmentProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductDevelopmentStatusEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDevelopmentStatusEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDevelopmentStatusEvents_ProductDevelopmentProjects_P~",
                        column: x => x.ProjectId,
                        principalTable: "ProductDevelopmentProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrototypeVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrototypeVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrototypeVersions_ProductDevelopmentProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "ProductDevelopmentProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrototypeVersions_Users_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrototypeFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrototypeVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrototypeFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrototypeFiles_PrototypeVersions_PrototypeVersionId",
                        column: x => x.PrototypeVersionId,
                        principalTable: "PrototypeVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductDevelopmentComments_AuthorUserId",
                table: "ProductDevelopmentComments",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDevelopmentComments_ProjectId",
                table: "ProductDevelopmentComments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDevelopmentMilestones_ProjectId",
                table: "ProductDevelopmentMilestones",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDevelopmentProjects_BusinessPartnerId",
                table: "ProductDevelopmentProjects",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDevelopmentProjects_FinalProductId",
                table: "ProductDevelopmentProjects",
                column: "FinalProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductDevelopmentProjects_ProducerId",
                table: "ProductDevelopmentProjects",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDevelopmentProjects_ReferenceNumber",
                table: "ProductDevelopmentProjects",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductDevelopmentStatusEvents_ProjectId",
                table: "ProductDevelopmentStatusEvents",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeFiles_PrototypeVersionId",
                table: "PrototypeFiles",
                column: "PrototypeVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeVersions_ProjectId_VersionNumber",
                table: "PrototypeVersions",
                columns: new[] { "ProjectId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeVersions_SubmittedByUserId",
                table: "PrototypeVersions",
                column: "SubmittedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductDevelopmentComments");

            migrationBuilder.DropTable(
                name: "ProductDevelopmentMilestones");

            migrationBuilder.DropTable(
                name: "ProductDevelopmentStatusEvents");

            migrationBuilder.DropTable(
                name: "PrototypeFiles");

            migrationBuilder.DropTable(
                name: "PrototypeVersions");

            migrationBuilder.DropTable(
                name: "ProductDevelopmentProjects");
        }
    }
}

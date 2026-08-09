using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignCollaborationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DesignCollaborationProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProducerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DesignRequirements = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignCollaborationProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignCollaborationProjects_Users_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DesignCollaborationProjects_Users_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollaborationStatusEvents",
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
                    table.PrimaryKey("PK_CollaborationStatusEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollaborationStatusEvents_DesignCollaborationProjects_Proje~",
                        column: x => x.ProjectId,
                        principalTable: "DesignCollaborationProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DesignComments",
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
                    table.PrimaryKey("PK_DesignComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignComments_DesignCollaborationProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "DesignCollaborationProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DesignComments_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DesignRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignRevisions_DesignCollaborationProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "DesignCollaborationProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DesignRevisions_Users_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DesignFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignFiles_DesignCollaborationProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "DesignCollaborationProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DesignFiles_DesignRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "DesignRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DesignFiles_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollaborationStatusEvents_ProjectId",
                table: "CollaborationStatusEvents",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignCollaborationProjects_BusinessPartnerId",
                table: "DesignCollaborationProjects",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignCollaborationProjects_ProducerId",
                table: "DesignCollaborationProjects",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignCollaborationProjects_ReferenceNumber",
                table: "DesignCollaborationProjects",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DesignComments_AuthorUserId",
                table: "DesignComments",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignComments_ProjectId",
                table: "DesignComments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignFiles_ProjectId",
                table: "DesignFiles",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignFiles_RevisionId",
                table: "DesignFiles",
                column: "RevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignFiles_UploadedByUserId",
                table: "DesignFiles",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignRevisions_ProjectId_RevisionNumber",
                table: "DesignRevisions",
                columns: new[] { "ProjectId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DesignRevisions_SubmittedByUserId",
                table: "DesignRevisions",
                column: "SubmittedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollaborationStatusEvents");

            migrationBuilder.DropTable(
                name: "DesignComments");

            migrationBuilder.DropTable(
                name: "DesignFiles");

            migrationBuilder.DropTable(
                name: "DesignRevisions");

            migrationBuilder.DropTable(
                name: "DesignCollaborationProjects");
        }
    }
}

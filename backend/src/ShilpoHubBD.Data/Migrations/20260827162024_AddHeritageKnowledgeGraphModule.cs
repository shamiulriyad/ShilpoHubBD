using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeritageKnowledgeGraphModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnowledgeNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LabelNormalized = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExternalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    IsCurated = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeNodes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsDirected = table.Column<bool>(type: "boolean", nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: true),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeRelationships_KnowledgeNodes_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "KnowledgeNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KnowledgeRelationships_KnowledgeNodes_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "KnowledgeNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KnowledgeRelationships_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeNodes_CreatedByUserId",
                table: "KnowledgeNodes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeNodes_NodeType",
                table: "KnowledgeNodes",
                column: "NodeType");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeNodes_NodeType_ExternalEntityId",
                table: "KnowledgeNodes",
                columns: new[] { "NodeType", "ExternalEntityId" },
                unique: true,
                filter: "\"ExternalEntityId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeNodes_NodeType_LabelNormalized",
                table: "KnowledgeNodes",
                columns: new[] { "NodeType", "LabelNormalized" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRelationships_CreatedByUserId",
                table: "KnowledgeRelationships",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRelationships_RelationshipType",
                table: "KnowledgeRelationships",
                column: "RelationshipType");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRelationships_SourceNodeId",
                table: "KnowledgeRelationships",
                column: "SourceNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRelationships_SourceNodeId_TargetNodeId_Relationsh~",
                table: "KnowledgeRelationships",
                columns: new[] { "SourceNodeId", "TargetNodeId", "RelationshipType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeRelationships_TargetNodeId",
                table: "KnowledgeRelationships",
                column: "TargetNodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnowledgeRelationships");

            migrationBuilder.DropTable(
                name: "KnowledgeNodes");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAIResearchAssistantModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResearchAIAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ResearchQuestions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    InputSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ContextJson = table.Column<string>(type: "character varying(32000)", maxLength: 32000, nullable: true),
                    ResultSummary = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    ResultJson = table.Column<string>(type: "character varying(64000)", maxLength: 64000, nullable: true),
                    Confidence = table.Column<double>(type: "double precision", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DatasetId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResearchPaperId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchAIAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchAIAnalyses_HeritageDatasets_DatasetId",
                        column: x => x.DatasetId,
                        principalTable: "HeritageDatasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ResearchAIAnalyses_ResearchPapers_ResearchPaperId",
                        column: x => x.ResearchPaperId,
                        principalTable: "ResearchPapers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ResearchAIAnalyses_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResearchAIAnalyses_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResearchAICitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchAIAnalysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Style = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceTitle = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Authors = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Container = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Doi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    FormattedCitation = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchAICitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchAICitations_ResearchAIAnalyses_ResearchAIAnalysisId",
                        column: x => x.ResearchAIAnalysisId,
                        principalTable: "ResearchAIAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResearchAICitations_ResearchPublications_ResearchPublicatio~",
                        column: x => x.ResearchPublicationId,
                        principalTable: "ResearchPublications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ResearchAIFindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchAIAnalysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Heading = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Detail = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Metric = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Score = table.Column<double>(type: "double precision", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchAIFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchAIFindings_ResearchAIAnalyses_ResearchAIAnalysisId",
                        column: x => x.ResearchAIAnalysisId,
                        principalTable: "ResearchAIAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchAIAnalyses_AnalysisType",
                table: "ResearchAIAnalyses",
                column: "AnalysisType");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchAIAnalyses_DatasetId",
                table: "ResearchAIAnalyses",
                column: "DatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchAIAnalyses_RequestedByUserId",
                table: "ResearchAIAnalyses",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchAIAnalyses_ResearchPaperId",
                table: "ResearchAIAnalyses",
                column: "ResearchPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchAIAnalyses_ResearchProjectId_CreatedAt",
                table: "ResearchAIAnalyses",
                columns: new[] { "ResearchProjectId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchAICitations_ResearchAIAnalysisId_DisplayOrder",
                table: "ResearchAICitations",
                columns: new[] { "ResearchAIAnalysisId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchAICitations_ResearchPublicationId",
                table: "ResearchAICitations",
                column: "ResearchPublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchAIFindings_ResearchAIAnalysisId_DisplayOrder",
                table: "ResearchAIFindings",
                columns: new[] { "ResearchAIAnalysisId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResearchAICitations");

            migrationBuilder.DropTable(
                name: "ResearchAIFindings");

            migrationBuilder.DropTable(
                name: "ResearchAIAnalyses");
        }
    }
}

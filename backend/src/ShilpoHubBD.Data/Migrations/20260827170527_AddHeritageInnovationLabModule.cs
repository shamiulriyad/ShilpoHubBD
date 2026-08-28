using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeritageInnovationLabModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreservationStrategies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    HeritageDatasetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HeritageProblem = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    ProposedSolution = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    ExpectedImpact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EvidenceReferences = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TargetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreservationStrategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreservationStrategies_HeritageDatasets_HeritageDatasetId",
                        column: x => x.HeritageDatasetId,
                        principalTable: "HeritageDatasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PreservationStrategies_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PreservationStrategies_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StrategyObjectives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreservationStrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    IsAchieved = table.Column<bool>(type: "boolean", nullable: false),
                    AchievedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyObjectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategyObjectives_PreservationStrategies_PreservationStrat~",
                        column: x => x.PreservationStrategyId,
                        principalTable: "PreservationStrategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategyActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreservationStrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyObjectiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrategyActions_PreservationStrategies_PreservationStrategy~",
                        column: x => x.PreservationStrategyId,
                        principalTable: "PreservationStrategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StrategyActions_StrategyObjectives_StrategyObjectiveId",
                        column: x => x.StrategyObjectiveId,
                        principalTable: "StrategyObjectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StrategyActions_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HeritageInnovationSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmitterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    InnovationPrototypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreservationStrategyId = table.Column<Guid>(type: "uuid", nullable: true),
                    HeritageDatasetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Problem = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    Solution = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    ResearchEvidence = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeritageInnovationSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeritageInnovationSubmissions_HeritageDatasets_HeritageData~",
                        column: x => x.HeritageDatasetId,
                        principalTable: "HeritageDatasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HeritageInnovationSubmissions_PreservationStrategies_Preser~",
                        column: x => x.PreservationStrategyId,
                        principalTable: "PreservationStrategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HeritageInnovationSubmissions_ResearchProjects_ResearchProj~",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HeritageInnovationSubmissions_Users_DecisionByUserId",
                        column: x => x.DecisionByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HeritageInnovationSubmissions_Users_SubmitterUserId",
                        column: x => x.SubmitterUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageInnovationSubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionEvents_HeritageInnovationSubmissions_HeritageInno~",
                        column: x => x.HeritageInnovationSubmissionId,
                        principalTable: "HeritageInnovationSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubmissionEvents_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageInnovationSubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    Comments = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionReviews_HeritageInnovationSubmissions_HeritageInn~",
                        column: x => x.HeritageInnovationSubmissionId,
                        principalTable: "HeritageInnovationSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubmissionReviews_Users_ReviewerUserId",
                        column: x => x.ReviewerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionTeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeritageInnovationSubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleOnTeam = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    AddedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionTeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionTeamMembers_HeritageInnovationSubmissions_Heritag~",
                        column: x => x.HeritageInnovationSubmissionId,
                        principalTable: "HeritageInnovationSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubmissionTeamMembers_Users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubmissionTeamMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InnovationExperiments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    HeritageDatasetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Objective = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ModelType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Framework = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConfigJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VersionCount = table.Column<int>(type: "integer", nullable: false),
                    RunCount = table.Column<int>(type: "integer", nullable: false),
                    CurrentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InnovationExperiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InnovationExperiments_HeritageDatasets_HeritageDatasetId",
                        column: x => x.HeritageDatasetId,
                        principalTable: "HeritageDatasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InnovationExperiments_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InnovationExperiments_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InnovationExperimentVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InnovationExperimentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ConfigJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    Framework = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ArtifactUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InnovationExperimentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InnovationExperimentVersions_InnovationExperiments_Innovati~",
                        column: x => x.InnovationExperimentId,
                        principalTable: "InnovationExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InnovationExperimentVersions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InnovationExperimentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExperimentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RunNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DatasetSnapshotName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    HyperparametersJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    MetricsJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    PrimaryMetricName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrimaryMetricValue = table.Column<double>(type: "double precision", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggeredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingRuns_InnovationExperimentVersions_ExperimentVersion~",
                        column: x => x.ExperimentVersionId,
                        principalTable: "InnovationExperimentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrainingRuns_InnovationExperiments_InnovationExperimentId",
                        column: x => x.InnovationExperimentId,
                        principalTable: "InnovationExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingRuns_Users_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InnovationPrototypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreservationStrategyId = table.Column<Guid>(type: "uuid", nullable: true),
                    InnovationExperimentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VersionCount = table.Column<int>(type: "integer", nullable: false),
                    CurrentIterationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InnovationPrototypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InnovationPrototypes_InnovationExperiments_InnovationExperi~",
                        column: x => x.InnovationExperimentId,
                        principalTable: "InnovationExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InnovationPrototypes_PreservationStrategies_PreservationStr~",
                        column: x => x.PreservationStrategyId,
                        principalTable: "PreservationStrategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InnovationPrototypes_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InnovationPrototypes_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrototypeIterations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InnovationPrototypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChangeSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ArtifactUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrototypeIterations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrototypeIterations_InnovationPrototypes_InnovationPrototyp~",
                        column: x => x.InnovationPrototypeId,
                        principalTable: "InnovationPrototypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrototypeIterations_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrototypeTestCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InnovationPrototypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Steps = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    ExpectedResult = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrototypeTestCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrototypeTestCases_InnovationPrototypes_InnovationPrototype~",
                        column: x => x.InnovationPrototypeId,
                        principalTable: "InnovationPrototypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrototypeTestRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InnovationPrototypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrototypeIterationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RunNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: true),
                    Environment = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalCases = table.Column<int>(type: "integer", nullable: false),
                    PassedCases = table.Column<int>(type: "integer", nullable: false),
                    FailedCases = table.Column<int>(type: "integer", nullable: false),
                    BlockedCases = table.Column<int>(type: "integer", nullable: false),
                    ExecutedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrototypeTestRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrototypeTestRuns_InnovationPrototypes_InnovationPrototypeId",
                        column: x => x.InnovationPrototypeId,
                        principalTable: "InnovationPrototypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrototypeTestRuns_PrototypeIterations_PrototypeIterationId",
                        column: x => x.PrototypeIterationId,
                        principalTable: "PrototypeIterations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrototypeTestRuns_Users_ExecutedByUserId",
                        column: x => x.ExecutedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrototypeIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InnovationPrototypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrototypeTestRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Resolution = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrototypeIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrototypeIssues_InnovationPrototypes_InnovationPrototypeId",
                        column: x => x.InnovationPrototypeId,
                        principalTable: "InnovationPrototypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrototypeIssues_PrototypeTestRuns_PrototypeTestRunId",
                        column: x => x.PrototypeTestRunId,
                        principalTable: "PrototypeTestRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrototypeIssues_Users_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrototypeIssues_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrototypeTestResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrototypeTestRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrototypeTestCaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    CaseTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ActualResult = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrototypeTestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrototypeTestResults_PrototypeTestCases_PrototypeTestCaseId",
                        column: x => x.PrototypeTestCaseId,
                        principalTable: "PrototypeTestCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrototypeTestResults_PrototypeTestRuns_PrototypeTestRunId",
                        column: x => x.PrototypeTestRunId,
                        principalTable: "PrototypeTestRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HeritageInnovationSubmissions_DecisionByUserId",
                table: "HeritageInnovationSubmissions",
                column: "DecisionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageInnovationSubmissions_HeritageDatasetId",
                table: "HeritageInnovationSubmissions",
                column: "HeritageDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageInnovationSubmissions_InnovationPrototypeId",
                table: "HeritageInnovationSubmissions",
                column: "InnovationPrototypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageInnovationSubmissions_PreservationStrategyId",
                table: "HeritageInnovationSubmissions",
                column: "PreservationStrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageInnovationSubmissions_ResearchProjectId",
                table: "HeritageInnovationSubmissions",
                column: "ResearchProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageInnovationSubmissions_Status",
                table: "HeritageInnovationSubmissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HeritageInnovationSubmissions_SubmitterUserId",
                table: "HeritageInnovationSubmissions",
                column: "SubmitterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationExperiments_CurrentVersionId",
                table: "InnovationExperiments",
                column: "CurrentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationExperiments_HeritageDatasetId",
                table: "InnovationExperiments",
                column: "HeritageDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationExperiments_OwnerUserId",
                table: "InnovationExperiments",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationExperiments_ResearchProjectId",
                table: "InnovationExperiments",
                column: "ResearchProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationExperiments_Status",
                table: "InnovationExperiments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationExperimentVersions_CreatedByUserId",
                table: "InnovationExperimentVersions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationExperimentVersions_InnovationExperimentId_Version~",
                table: "InnovationExperimentVersions",
                columns: new[] { "InnovationExperimentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InnovationPrototypes_CurrentIterationId",
                table: "InnovationPrototypes",
                column: "CurrentIterationId");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationPrototypes_InnovationExperimentId",
                table: "InnovationPrototypes",
                column: "InnovationExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationPrototypes_OwnerUserId",
                table: "InnovationPrototypes",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationPrototypes_PreservationStrategyId",
                table: "InnovationPrototypes",
                column: "PreservationStrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationPrototypes_ResearchProjectId",
                table: "InnovationPrototypes",
                column: "ResearchProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_InnovationPrototypes_Status",
                table: "InnovationPrototypes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PreservationStrategies_HeritageDatasetId",
                table: "PreservationStrategies",
                column: "HeritageDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_PreservationStrategies_OwnerUserId",
                table: "PreservationStrategies",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PreservationStrategies_ResearchProjectId",
                table: "PreservationStrategies",
                column: "ResearchProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PreservationStrategies_Status",
                table: "PreservationStrategies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeIssues_InnovationPrototypeId_Status",
                table: "PrototypeIssues",
                columns: new[] { "InnovationPrototypeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeIssues_PrototypeTestRunId",
                table: "PrototypeIssues",
                column: "PrototypeTestRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeIssues_ReportedByUserId",
                table: "PrototypeIssues",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeIssues_ResolvedByUserId",
                table: "PrototypeIssues",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeIssues_Severity",
                table: "PrototypeIssues",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeIterations_CreatedByUserId",
                table: "PrototypeIterations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeIterations_InnovationPrototypeId_VersionNumber",
                table: "PrototypeIterations",
                columns: new[] { "InnovationPrototypeId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTestCases_InnovationPrototypeId_OrderIndex",
                table: "PrototypeTestCases",
                columns: new[] { "InnovationPrototypeId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTestResults_PrototypeTestCaseId",
                table: "PrototypeTestResults",
                column: "PrototypeTestCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTestResults_PrototypeTestRunId",
                table: "PrototypeTestResults",
                column: "PrototypeTestRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTestRuns_ExecutedByUserId",
                table: "PrototypeTestRuns",
                column: "ExecutedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTestRuns_InnovationPrototypeId_RunNumber",
                table: "PrototypeTestRuns",
                columns: new[] { "InnovationPrototypeId", "RunNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTestRuns_PrototypeIterationId",
                table: "PrototypeTestRuns",
                column: "PrototypeIterationId");

            migrationBuilder.CreateIndex(
                name: "IX_PrototypeTestRuns_Status",
                table: "PrototypeTestRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyActions_AssignedToUserId",
                table: "StrategyActions",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyActions_PreservationStrategyId_OrderIndex",
                table: "StrategyActions",
                columns: new[] { "PreservationStrategyId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategyActions_Status",
                table: "StrategyActions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyActions_StrategyObjectiveId",
                table: "StrategyActions",
                column: "StrategyObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyObjectives_PreservationStrategyId_OrderIndex",
                table: "StrategyObjectives",
                columns: new[] { "PreservationStrategyId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionEvents_ActorUserId",
                table: "SubmissionEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionEvents_HeritageInnovationSubmissionId_CreatedAt",
                table: "SubmissionEvents",
                columns: new[] { "HeritageInnovationSubmissionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionReviews_HeritageInnovationSubmissionId",
                table: "SubmissionReviews",
                column: "HeritageInnovationSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionReviews_ReviewerUserId",
                table: "SubmissionReviews",
                column: "ReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionTeamMembers_AddedByUserId",
                table: "SubmissionTeamMembers",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionTeamMembers_HeritageInnovationSubmissionId_UserId",
                table: "SubmissionTeamMembers",
                columns: new[] { "HeritageInnovationSubmissionId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionTeamMembers_UserId",
                table: "SubmissionTeamMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRuns_ExperimentVersionId",
                table: "TrainingRuns",
                column: "ExperimentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRuns_InnovationExperimentId_RunNumber",
                table: "TrainingRuns",
                columns: new[] { "InnovationExperimentId", "RunNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRuns_Status",
                table: "TrainingRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRuns_TriggeredByUserId",
                table: "TrainingRuns",
                column: "TriggeredByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HeritageInnovationSubmissions_InnovationPrototypes_Innovati~",
                table: "HeritageInnovationSubmissions",
                column: "InnovationPrototypeId",
                principalTable: "InnovationPrototypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InnovationExperiments_InnovationExperimentVersions_CurrentV~",
                table: "InnovationExperiments",
                column: "CurrentVersionId",
                principalTable: "InnovationExperimentVersions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InnovationPrototypes_PrototypeIterations_CurrentIterationId",
                table: "InnovationPrototypes",
                column: "CurrentIterationId",
                principalTable: "PrototypeIterations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrototypeIterations_InnovationPrototypes_InnovationPrototyp~",
                table: "PrototypeIterations");

            migrationBuilder.DropForeignKey(
                name: "FK_InnovationExperiments_InnovationExperimentVersions_CurrentV~",
                table: "InnovationExperiments");

            migrationBuilder.DropTable(
                name: "PrototypeIssues");

            migrationBuilder.DropTable(
                name: "PrototypeTestResults");

            migrationBuilder.DropTable(
                name: "StrategyActions");

            migrationBuilder.DropTable(
                name: "SubmissionEvents");

            migrationBuilder.DropTable(
                name: "SubmissionReviews");

            migrationBuilder.DropTable(
                name: "SubmissionTeamMembers");

            migrationBuilder.DropTable(
                name: "TrainingRuns");

            migrationBuilder.DropTable(
                name: "PrototypeTestCases");

            migrationBuilder.DropTable(
                name: "PrototypeTestRuns");

            migrationBuilder.DropTable(
                name: "StrategyObjectives");

            migrationBuilder.DropTable(
                name: "HeritageInnovationSubmissions");

            migrationBuilder.DropTable(
                name: "InnovationPrototypes");

            migrationBuilder.DropTable(
                name: "PreservationStrategies");

            migrationBuilder.DropTable(
                name: "PrototypeIterations");

            migrationBuilder.DropTable(
                name: "InnovationExperimentVersions");

            migrationBuilder.DropTable(
                name: "InnovationExperiments");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    TimeLimitMinutes = table.Column<int>(type: "integer", nullable: true),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: true),
                    PassingScorePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exams_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    TimeLimitMinutes = table.Column<int>(type: "integer", nullable: true),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: true),
                    PassingScorePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quizzes_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionText = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    Feedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GradedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_Users_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    PercentageScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EvaluatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamAttempts_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamAttempts_Users_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamQuestions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    PercentageScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Users_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamQuestionOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamQuestionOptions_ExamQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "ExamQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestionOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizQuestionOptions_QuizQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamAttemptAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EssayAnswerText = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: true),
                    PointsAwarded = table.Column<int>(type: "integer", nullable: true),
                    Feedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamAttemptAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamAttemptAnswers_ExamAttempts_ExamAttemptId",
                        column: x => x.ExamAttemptId,
                        principalTable: "ExamAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamAttemptAnswers_ExamQuestionOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "ExamQuestionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamAttemptAnswers_ExamQuestions_ExamQuestionId",
                        column: x => x.ExamQuestionId,
                        principalTable: "ExamQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttemptAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: true),
                    PointsAwarded = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttemptAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswers_QuizAttempts_QuizAttemptId",
                        column: x => x.QuizAttemptId,
                        principalTable: "QuizAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswers_QuizQuestionOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "QuizQuestionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswers_QuizQuestions_QuizQuestionId",
                        column: x => x.QuizQuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_CourseId",
                table: "Assignments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_AssignmentId_StudentUserId",
                table: "AssignmentSubmissions",
                columns: new[] { "AssignmentId", "StudentUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_StudentUserId",
                table: "AssignmentSubmissions",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttemptAnswers_ExamAttemptId_ExamQuestionId",
                table: "ExamAttemptAnswers",
                columns: new[] { "ExamAttemptId", "ExamQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttemptAnswers_ExamQuestionId",
                table: "ExamAttemptAnswers",
                column: "ExamQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttemptAnswers_SelectedOptionId",
                table: "ExamAttemptAnswers",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttempts_ExamId_StudentUserId",
                table: "ExamAttempts",
                columns: new[] { "ExamId", "StudentUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamAttempts_StudentUserId",
                table: "ExamAttempts",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestionOptions_QuestionId",
                table: "ExamQuestionOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestions_ExamId_DisplayOrder",
                table: "ExamQuestions",
                columns: new[] { "ExamId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CourseId",
                table: "Exams",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_QuizAttemptId_QuizQuestionId",
                table: "QuizAttemptAnswers",
                columns: new[] { "QuizAttemptId", "QuizQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_QuizQuestionId",
                table: "QuizAttemptAnswers",
                column: "QuizQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_SelectedOptionId",
                table: "QuizAttemptAnswers",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizId_StudentUserId",
                table: "QuizAttempts",
                columns: new[] { "QuizId", "StudentUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_StudentUserId",
                table: "QuizAttempts",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestionOptions_QuestionId",
                table: "QuizQuestionOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_QuizId_DisplayOrder",
                table: "QuizQuestions",
                columns: new[] { "QuizId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_CourseId",
                table: "Quizzes",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentSubmissions");

            migrationBuilder.DropTable(
                name: "ExamAttemptAnswers");

            migrationBuilder.DropTable(
                name: "QuizAttemptAnswers");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "ExamAttempts");

            migrationBuilder.DropTable(
                name: "ExamQuestionOptions");

            migrationBuilder.DropTable(
                name: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "QuizQuestionOptions");

            migrationBuilder.DropTable(
                name: "ExamQuestions");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "Quizzes");
        }
    }
}

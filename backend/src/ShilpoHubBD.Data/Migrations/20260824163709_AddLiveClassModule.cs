using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveClassModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiveClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstructorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    MeetingUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MaxParticipants = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScheduledStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveClasses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LiveClasses_Users_InstructorUserId",
                        column: x => x.InstructorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LiveClassAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LiveClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveClassAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveClassAttendances_LiveClasses_LiveClassId",
                        column: x => x.LiveClassId,
                        principalTable: "LiveClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveClassAttendances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveClassParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LiveClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveClassParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveClassParticipants_LiveClasses_LiveClassId",
                        column: x => x.LiveClassId,
                        principalTable: "LiveClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveClassParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveClassQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LiveClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsAnswered = table.Column<bool>(type: "boolean", nullable: false),
                    AnswerBody = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveClassQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveClassQuestions_LiveClasses_LiveClassId",
                        column: x => x.LiveClassId,
                        principalTable: "LiveClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveClassQuestions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveClassAttendances_LiveClassId_UserId",
                table: "LiveClassAttendances",
                columns: new[] { "LiveClassId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_LiveClassAttendances_UserId",
                table: "LiveClassAttendances",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveClasses_CourseId",
                table: "LiveClasses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveClasses_InstructorUserId",
                table: "LiveClasses",
                column: "InstructorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveClasses_ScheduledStartAt",
                table: "LiveClasses",
                column: "ScheduledStartAt");

            migrationBuilder.CreateIndex(
                name: "IX_LiveClasses_Status",
                table: "LiveClasses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LiveClassParticipants_LiveClassId_UserId",
                table: "LiveClassParticipants",
                columns: new[] { "LiveClassId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LiveClassParticipants_UserId",
                table: "LiveClassParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveClassQuestions_LiveClassId",
                table: "LiveClassQuestions",
                column: "LiveClassId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveClassQuestions_UserId",
                table: "LiveClassQuestions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveClassAttendances");

            migrationBuilder.DropTable(
                name: "LiveClassParticipants");

            migrationBuilder.DropTable(
                name: "LiveClassQuestions");

            migrationBuilder.DropTable(
                name: "LiveClasses");
        }
    }
}

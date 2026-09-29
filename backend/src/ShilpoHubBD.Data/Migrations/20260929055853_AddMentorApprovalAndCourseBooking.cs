using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMentorApprovalAndCourseBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "MentorProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProofImageUrl",
                table: "MentorProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "MentorProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "MentorProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "MentorProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassTime",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysPerWeek",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryMode",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Courses",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SessionMinutes",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttendanceMode",
                table: "CourseEnrollments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "FeeAmount",
                table: "CourseEnrollments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "CourseEnrollments",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Preserve existing mentor/course access while requiring review for new applications.
            migrationBuilder.Sql("""
                UPDATE "MentorProfiles" SET "ApprovalStatus" = CASE WHEN "IsActive" THEN 'Approved' ELSE 'Pending' END,
                    "ReviewNote" = 'Existing mentor profile retained during Academy upgrade; no new review claimed.';
                UPDATE "Courses" SET "ClassTime" = '10:00', "DaysPerWeek" = 1, "DeliveryMode" = 'Online',
                    "DurationDays" = 1, "SessionMinutes" = 60;
                UPDATE "CourseEnrollments" SET "AttendanceMode" = 'Online', "PaymentStatus" = 'Free';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "MentorProfiles");

            migrationBuilder.DropColumn(
                name: "ProofImageUrl",
                table: "MentorProfiles");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "MentorProfiles");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "MentorProfiles");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "MentorProfiles");

            migrationBuilder.DropColumn(
                name: "ClassTime",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DaysPerWeek",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DeliveryMode",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DurationDays",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SessionMinutes",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Venue",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "AttendanceMode",
                table: "CourseEnrollments");

            migrationBuilder.DropColumn(
                name: "FeeAmount",
                table: "CourseEnrollments");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "CourseEnrollments");
        }
    }
}

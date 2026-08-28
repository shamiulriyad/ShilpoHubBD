using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendCertificateModuleForSkillAndApprenticeship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainingCertificates_EnrollmentId",
                table: "TrainingCertificates");

            migrationBuilder.RenameColumn(
                name: "CourseTitle",
                table: "TrainingCertificates",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "ApprenticeName",
                table: "TrainingCertificates",
                newName: "RecipientName");

            migrationBuilder.RenameColumn(
                name: "MentorName",
                table: "TrainingCertificates",
                newName: "IssuerName");

            migrationBuilder.AlterColumn<Guid>(
                name: "EnrollmentId",
                table: "TrainingCertificates",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ApprenticeEnrollmentId",
                table: "TrainingCertificates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HeritageSkillId",
                table: "TrainingCertificates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IssuerUserId",
                table: "TrainingCertificates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecipientUserId",
                table: "TrainingCertificates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "TrainingCertificates",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Course");

            // Backfill the new recipient/issuer columns for any certificates issued before this
            // migration, deriving them from the course enrollment they were originally tied to.
            migrationBuilder.Sql(
                """
                UPDATE "TrainingCertificates" tc
                SET "RecipientUserId" = ce."ApprenticeId"
                FROM "CourseEnrollments" ce
                WHERE tc."EnrollmentId" = ce."Id";

                UPDATE "TrainingCertificates" tc
                SET "IssuerUserId" = COALESCE(mp."UserId", amp."UserId")
                FROM "CourseEnrollments" ce
                JOIN "Courses" c ON c."Id" = ce."CourseId"
                LEFT JOIN "MentorProfiles" mp ON mp."Id" = c."MentorId"
                LEFT JOIN "AcademyMemberProfiles" amp ON amp."Id" = c."TrainerProfileId"
                WHERE tc."EnrollmentId" = ce."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertificates_ApprenticeEnrollmentId",
                table: "TrainingCertificates",
                column: "ApprenticeEnrollmentId",
                unique: true,
                filter: "\"ApprenticeEnrollmentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertificates_EnrollmentId",
                table: "TrainingCertificates",
                column: "EnrollmentId",
                unique: true,
                filter: "\"EnrollmentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertificates_HeritageSkillId",
                table: "TrainingCertificates",
                column: "HeritageSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertificates_IssuerUserId",
                table: "TrainingCertificates",
                column: "IssuerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertificates_RecipientUserId",
                table: "TrainingCertificates",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertificates_RecipientUserId_HeritageSkillId_Type",
                table: "TrainingCertificates",
                columns: new[] { "RecipientUserId", "HeritageSkillId", "Type" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingCertificates_ApprenticeEnrollments_ApprenticeEnroll~",
                table: "TrainingCertificates",
                column: "ApprenticeEnrollmentId",
                principalTable: "ApprenticeEnrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingCertificates_HeritageSkills_HeritageSkillId",
                table: "TrainingCertificates",
                column: "HeritageSkillId",
                principalTable: "HeritageSkills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingCertificates_Users_IssuerUserId",
                table: "TrainingCertificates",
                column: "IssuerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingCertificates_Users_RecipientUserId",
                table: "TrainingCertificates",
                column: "RecipientUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingCertificates_ApprenticeEnrollments_ApprenticeEnroll~",
                table: "TrainingCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingCertificates_HeritageSkills_HeritageSkillId",
                table: "TrainingCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingCertificates_Users_IssuerUserId",
                table: "TrainingCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingCertificates_Users_RecipientUserId",
                table: "TrainingCertificates");

            migrationBuilder.DropIndex(
                name: "IX_TrainingCertificates_ApprenticeEnrollmentId",
                table: "TrainingCertificates");

            migrationBuilder.DropIndex(
                name: "IX_TrainingCertificates_EnrollmentId",
                table: "TrainingCertificates");

            migrationBuilder.DropIndex(
                name: "IX_TrainingCertificates_HeritageSkillId",
                table: "TrainingCertificates");

            migrationBuilder.DropIndex(
                name: "IX_TrainingCertificates_IssuerUserId",
                table: "TrainingCertificates");

            migrationBuilder.DropIndex(
                name: "IX_TrainingCertificates_RecipientUserId",
                table: "TrainingCertificates");

            migrationBuilder.DropIndex(
                name: "IX_TrainingCertificates_RecipientUserId_HeritageSkillId_Type",
                table: "TrainingCertificates");

            migrationBuilder.DropColumn(
                name: "ApprenticeEnrollmentId",
                table: "TrainingCertificates");

            migrationBuilder.DropColumn(
                name: "HeritageSkillId",
                table: "TrainingCertificates");

            migrationBuilder.DropColumn(
                name: "IssuerUserId",
                table: "TrainingCertificates");

            migrationBuilder.DropColumn(
                name: "RecipientUserId",
                table: "TrainingCertificates");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TrainingCertificates");

            migrationBuilder.RenameColumn(
                name: "IssuerName",
                table: "TrainingCertificates",
                newName: "MentorName");

            migrationBuilder.RenameColumn(
                name: "RecipientName",
                table: "TrainingCertificates",
                newName: "ApprenticeName");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "TrainingCertificates",
                newName: "CourseTitle");

            migrationBuilder.AlterColumn<Guid>(
                name: "EnrollmentId",
                table: "TrainingCertificates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertificates_EnrollmentId",
                table: "TrainingCertificates",
                column: "EnrollmentId",
                unique: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShilpoHubBD.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ShilpoHubDbContext))]
    [Migration("20260929170000_DisableProductApprovalGate")]
    public partial class DisableProductApprovalGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Products"
                SET "ApprovalStatus" = 'Approved',
                    "ApprovedAt" = COALESCE("ApprovedAt", NOW()),
                    "ApprovedByUserId" = NULL,
                    "RejectionReason" = NULL,
                    "UpdatedAt" = NOW()
                WHERE "ApprovalStatus" = 'Pending';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Previous pending decisions cannot be reconstructed safely.
        }
    }
}

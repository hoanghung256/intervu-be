using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProfileCertification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateCertificates_CandidateProfiles_CandidateProfileId1",
                table: "CandidateCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_CoachCertificates_CoachProfiles_CoachProfileId1",
                table: "CoachCertificates");

            migrationBuilder.DropIndex(
                name: "IX_CoachCertificates_CoachProfileId1",
                table: "CoachCertificates");

            migrationBuilder.DropIndex(
                name: "IX_CandidateCertificates_CandidateProfileId1",
                table: "CandidateCertificates");

            migrationBuilder.DropColumn(
                name: "CoachProfileId1",
                table: "CoachCertificates");

            migrationBuilder.DropColumn(
                name: "CandidateProfileId1",
                table: "CandidateCertificates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CoachProfileId1",
                table: "CoachCertificates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CandidateProfileId1",
                table: "CandidateCertificates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoachCertificates_CoachProfileId1",
                table: "CoachCertificates",
                column: "CoachProfileId1");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCertificates_CandidateProfileId1",
                table: "CandidateCertificates",
                column: "CandidateProfileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateCertificates_CandidateProfiles_CandidateProfileId1",
                table: "CandidateCertificates",
                column: "CandidateProfileId1",
                principalTable: "CandidateProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CoachCertificates_CoachProfiles_CoachProfileId1",
                table: "CoachCertificates",
                column: "CoachProfileId1",
                principalTable: "CoachProfiles",
                principalColumn: "Id");
        }
    }
}

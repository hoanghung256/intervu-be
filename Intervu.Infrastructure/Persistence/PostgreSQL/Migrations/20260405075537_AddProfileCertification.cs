using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileCertification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificationLinks",
                table: "CoachProfiles");

            migrationBuilder.DropColumn(
                name: "CertificationLinks",
                table: "CandidateProfiles");

            migrationBuilder.AddColumn<string>(
                name: "JobType",
                table: "CoachWorkExperiences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "CoachWorkExperiences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationType",
                table: "CoachWorkExperiences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionTitle",
                table: "CoachWorkExperiences",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobType",
                table: "CandidateWorkExperiences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "CandidateWorkExperiences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationType",
                table: "CandidateWorkExperiences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionTitle",
                table: "CandidateWorkExperiences",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CandidateCertificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Issuer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Link = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CandidateProfileId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateCertificates_CandidateProfiles_CandidateProfileId",
                        column: x => x.CandidateProfileId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateCertificates_CandidateProfiles_CandidateProfileId1",
                        column: x => x.CandidateProfileId1,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CoachCertificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Issuer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Link = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CoachProfileId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachCertificates_CoachProfiles_CoachProfileId",
                        column: x => x.CoachProfileId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachCertificates_CoachProfiles_CoachProfileId1",
                        column: x => x.CoachProfileId1,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCertificates_CandidateProfileId",
                table: "CandidateCertificates",
                column: "CandidateProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateCertificates_CandidateProfileId1",
                table: "CandidateCertificates",
                column: "CandidateProfileId1");

            migrationBuilder.CreateIndex(
                name: "IX_CoachCertificates_CoachProfileId",
                table: "CoachCertificates",
                column: "CoachProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachCertificates_CoachProfileId1",
                table: "CoachCertificates",
                column: "CoachProfileId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateCertificates");

            migrationBuilder.DropTable(
                name: "CoachCertificates");

            migrationBuilder.DropColumn(
                name: "JobType",
                table: "CoachWorkExperiences");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "CoachWorkExperiences");

            migrationBuilder.DropColumn(
                name: "LocationType",
                table: "CoachWorkExperiences");

            migrationBuilder.DropColumn(
                name: "PositionTitle",
                table: "CoachWorkExperiences");

            migrationBuilder.DropColumn(
                name: "JobType",
                table: "CandidateWorkExperiences");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "CandidateWorkExperiences");

            migrationBuilder.DropColumn(
                name: "LocationType",
                table: "CandidateWorkExperiences");

            migrationBuilder.DropColumn(
                name: "PositionTitle",
                table: "CandidateWorkExperiences");

            migrationBuilder.AddColumn<string>(
                name: "CertificationLinks",
                table: "CoachProfiles",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificationLinks",
                table: "CandidateProfiles",
                type: "jsonb",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CandidateProfiles",
                keyColumn: "Id",
                keyValue: new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"),
                column: "CertificationLinks",
                value: null);

            migrationBuilder.UpdateData(
                table: "CoachProfiles",
                keyColumn: "Id",
                keyValue: new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"),
                column: "CertificationLinks",
                value: null);

            migrationBuilder.UpdateData(
                table: "CoachProfiles",
                keyColumn: "Id",
                keyValue: new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"),
                column: "CertificationLinks",
                value: null);

            migrationBuilder.UpdateData(
                table: "CoachProfiles",
                keyColumn: "Id",
                keyValue: new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"),
                column: "CertificationLinks",
                value: null);
        }
    }
}

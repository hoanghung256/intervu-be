using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileWorkingExperience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Re-point FK references before deleting the old seed row to avoid
            // FK constraint violation (Restrict delete on CoachInterviewServices).
            // The replacement row "...25cf" will receive the same InterviewType after the update below.
            migrationBuilder.Sql(
                """
                UPDATE "BookingRequests"
                SET "CoachInterviewServiceId" = '019d1467-d415-79f8-9bdc-5bb25a0b25cf'
                WHERE "CoachInterviewServiceId" = '019d1467-d415-7224-8808-39aa3e3b6377';

                UPDATE "InterviewRounds"
                SET "CoachInterviewServiceId" = '019d1467-d415-79f8-9bdc-5bb25a0b25cf'
                WHERE "CoachInterviewServiceId" = '019d1467-d415-7224-8808-39aa3e3b6377';

                UPDATE "InterviewRooms"
                SET "CoachInterviewServiceId" = '019d1467-d415-79f8-9bdc-5bb25a0b25cf'
                WHERE "CoachInterviewServiceId" = '019d1467-d415-7224-8808-39aa3e3b6377';
                """);

            migrationBuilder.DeleteData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1467-d415-7224-8808-39aa3e3b6377"));

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

            migrationBuilder.CreateTable(
                name: "CandidateIndustries",
                columns: table => new
                {
                    CandidateProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndustriesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateIndustries", x => new { x.CandidateProfilesId, x.IndustriesId });
                    table.ForeignKey(
                        name: "FK_CandidateIndustries_CandidateProfiles_CandidateProfilesId",
                        column: x => x.CandidateProfilesId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateIndustries_Industries_IndustriesId",
                        column: x => x.IndustriesId,
                        principalTable: "Industries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidateWorkExperiences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCurrentWorking = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnded = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SkillIds = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateWorkExperiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateWorkExperiences_CandidateProfiles_CandidateProfile~",
                        column: x => x.CandidateProfileId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachWorkExperiences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCurrentWorking = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnded = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SkillIds = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachWorkExperiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachWorkExperiences_CoachProfiles_CoachProfileId",
                        column: x => x.CoachProfileId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "CandidateProfiles",
                keyColumn: "Id",
                keyValue: new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"),
                column: "CertificationLinks",
                value: null);

            migrationBuilder.UpdateData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1467-d415-79f8-9bdc-5bb25a0b25cf"),
                columns: new[] { "DurationMinutes", "InterviewTypeId" },
                values: new object[] { 45, new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e") });

            migrationBuilder.InsertData(
                table: "CoachInterviewServices",
                columns: new[] { "Id", "CoachId", "DurationMinutes", "InterviewTypeId", "Price" },
                values: new object[] { new Guid("019d1467-d415-79f8-9bdc-5bb25a0b25cd"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 75, new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"), 2000 });

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

            migrationBuilder.CreateIndex(
                name: "IX_CandidateIndustries_IndustriesId",
                table: "CandidateIndustries",
                column: "IndustriesId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateWorkExperiences_CandidateProfileId",
                table: "CandidateWorkExperiences",
                column: "CandidateProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachWorkExperiences_CoachProfileId",
                table: "CoachWorkExperiences",
                column: "CoachProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateIndustries");

            migrationBuilder.DropTable(
                name: "CandidateWorkExperiences");

            migrationBuilder.DropTable(
                name: "CoachWorkExperiences");

            migrationBuilder.DeleteData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1467-d415-79f8-9bdc-5bb25a0b25cd"));

            migrationBuilder.DropColumn(
                name: "CertificationLinks",
                table: "CoachProfiles");

            migrationBuilder.DropColumn(
                name: "CertificationLinks",
                table: "CandidateProfiles");

            migrationBuilder.UpdateData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1467-d415-79f8-9bdc-5bb25a0b25cf"),
                columns: new[] { "DurationMinutes", "InterviewTypeId" },
                values: new object[] { 75, new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08") });

            migrationBuilder.InsertData(
                table: "CoachInterviewServices",
                columns: new[] { "Id", "CoachId", "DurationMinutes", "InterviewTypeId", "Price" },
                values: new object[] { new Guid("019d1467-d415-7224-8808-39aa3e3b6377"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 45, new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"), 2000 });
        }
    }
}

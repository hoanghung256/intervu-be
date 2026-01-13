using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInterviewerToCoach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_CandidateProfiles_StudentId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_InterviewerProfiles_InterviewerId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewRooms_CandidateProfiles_StudentId",
                table: "InterviewRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewRooms_InterviewerProfiles_InterviewerId",
                table: "InterviewRooms");

            migrationBuilder.DropTable(
                name: "InterviewerAvailabilities");

            migrationBuilder.DropTable(
                name: "InterviewerCompanies");

            migrationBuilder.DropTable(
                name: "InterviewerSkills");

            migrationBuilder.DropTable(
                name: "InterviewerProfiles");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "InterviewRooms",
                newName: "CoachId");

            migrationBuilder.RenameColumn(
                name: "InterviewerId",
                table: "InterviewRooms",
                newName: "CandidateId");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewRooms_StudentId",
                table: "InterviewRooms",
                newName: "IX_InterviewRooms_CoachId");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewRooms_InterviewerId",
                table: "InterviewRooms",
                newName: "IX_InterviewRooms_CandidateId");

            migrationBuilder.RenameColumn(
                name: "InterviewerAvailabilityId",
                table: "InterviewBookingTransaction",
                newName: "CoachAvailabilityId");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Feedbacks",
                newName: "CoachId");

            migrationBuilder.RenameColumn(
                name: "InterviewerId",
                table: "Feedbacks",
                newName: "CandidateId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_StudentId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_CoachId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_InterviewerId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_CandidateId");

            migrationBuilder.CreateTable(
                name: "CoachProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioUrl = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CurrentAmount = table.Column<int>(type: "integer", nullable: true),
                    ExperienceYears = table.Column<int>(type: "integer", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: false),
                    BankBinNumber = table.Column<string>(type: "text", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachProfiles_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachAvailabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsBooked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachAvailabilities_CoachProfiles_CoachId",
                        column: x => x.CoachId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachCompanies",
                columns: table => new
                {
                    CoachProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompaniesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachCompanies", x => new { x.CoachProfilesId, x.CompaniesId });
                    table.ForeignKey(
                        name: "FK_CoachCompanies_CoachProfiles_CoachProfilesId",
                        column: x => x.CoachProfilesId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachCompanies_Companies_CompaniesId",
                        column: x => x.CompaniesId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachSkills",
                columns: table => new
                {
                    CoachProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachSkills", x => new { x.CoachProfilesId, x.SkillsId });
                    table.ForeignKey(
                        name: "FK_CoachSkills_CoachProfiles_CoachProfilesId",
                        column: x => x.CoachProfilesId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachSkills_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CoachProfiles",
                columns: new[] { "Id", "BankAccountNumber", "BankBinNumber", "Bio", "CurrentAmount", "ExperienceYears", "PortfolioUrl", "Status" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "", "", "Senior Backend Engineer with real interview experience", 0, 8, "https://portfolio.example.com/bob", 0 },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), "", "", "Fullstack Engineer previously at Uber", 0, 6, "https://portfolio.example.com/john", 0 },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), "", "", "Senior Frontend Engineer focusing on UI/UX interviews", 0, 7, "https://portfolio.example.com/sarah", 0 }
                });

            migrationBuilder.UpdateData(
                table: "Feedbacks",
                keyColumn: "Id",
                keyValue: new Guid("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10"),
                columns: new[] { "CandidateId", "CoachId" },
                values: new object[] { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22") });

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"),
                columns: new[] { "CandidateId", "CoachId" },
                values: new object[] { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"),
                columns: new[] { "FullName", "SlugProfileUrl" },
                values: new object[] { "Alice Candidate", "alice-candidate_1719000000001" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"),
                columns: new[] { "FullName", "SlugProfileUrl" },
                values: new object[] { "Bob Coach", "bob-Coach_1719000000002" });

            migrationBuilder.InsertData(
                table: "CoachAvailabilities",
                columns: new[] { "Id", "CoachId", "EndTime", "IsBooked", "StartTime" },
                values: new object[] { new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2025, 11, 1, 10, 0, 0, 0, DateTimeKind.Utc), false, new DateTime(2025, 11, 1, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "CoachCompanies",
                columns: new[] { "CoachProfilesId", "CompaniesId" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("11111111-1111-4111-8111-111111111111") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("44444444-4444-4444-8444-444444444444") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("33333333-3333-4333-8333-333333333333") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("66666666-6666-4666-8666-666666666666") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("88888888-8888-4888-8888-888888888888") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("22222222-2222-4222-8222-222222222222") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("77777777-7777-4777-8777-777777777777") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("99999999-9999-4999-8999-999999999999") }
                });

            migrationBuilder.InsertData(
                table: "CoachSkills",
                columns: new[] { "CoachProfilesId", "SkillsId" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("02020202-0202-4202-8202-020202020202") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("06060606-0606-4606-8606-060606060606") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("07070707-0707-4707-8707-070707070707") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("08080808-0808-4808-8808-080808080808") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("04040404-0404-4404-8404-040404040404") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("07070707-0707-4707-8707-070707070707") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("09090909-0909-4909-8909-090909090909") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("0a0a0a0a-0a0a-4a0a-8a0a-0a0a0a0a0a0a") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("f5f5f5f5-f5f5-45f5-85f5-f5f5f5f5f5f5") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_CoachId",
                table: "CoachAvailabilities",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachCompanies_CompaniesId",
                table: "CoachCompanies",
                column: "CompaniesId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachSkills_SkillsId",
                table: "CoachSkills",
                column: "SkillsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_CandidateProfiles_CandidateId",
                table: "Feedbacks",
                column: "CandidateId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_CoachProfiles_CoachId",
                table: "Feedbacks",
                column: "CoachId",
                principalTable: "CoachProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewRooms_CandidateProfiles_CandidateId",
                table: "InterviewRooms",
                column: "CandidateId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewRooms_CoachProfiles_CoachId",
                table: "InterviewRooms",
                column: "CoachId",
                principalTable: "CoachProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_CandidateProfiles_CandidateId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_CoachProfiles_CoachId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewRooms_CandidateProfiles_CandidateId",
                table: "InterviewRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewRooms_CoachProfiles_CoachId",
                table: "InterviewRooms");

            migrationBuilder.DropTable(
                name: "CoachAvailabilities");

            migrationBuilder.DropTable(
                name: "CoachCompanies");

            migrationBuilder.DropTable(
                name: "CoachSkills");

            migrationBuilder.DropTable(
                name: "CoachProfiles");

            migrationBuilder.RenameColumn(
                name: "CoachId",
                table: "InterviewRooms",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "CandidateId",
                table: "InterviewRooms",
                newName: "InterviewerId");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewRooms_CoachId",
                table: "InterviewRooms",
                newName: "IX_InterviewRooms_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewRooms_CandidateId",
                table: "InterviewRooms",
                newName: "IX_InterviewRooms_InterviewerId");

            migrationBuilder.RenameColumn(
                name: "CoachAvailabilityId",
                table: "InterviewBookingTransaction",
                newName: "InterviewerAvailabilityId");

            migrationBuilder.RenameColumn(
                name: "CoachId",
                table: "Feedbacks",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "CandidateId",
                table: "Feedbacks",
                newName: "InterviewerId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_CoachId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_CandidateId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_InterviewerId");

            migrationBuilder.CreateTable(
                name: "InterviewerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "text", nullable: false),
                    BankBinNumber = table.Column<string>(type: "text", nullable: false),
                    Bio = table.Column<string>(type: "text", nullable: false),
                    CurrentAmount = table.Column<int>(type: "integer", nullable: true),
                    ExperienceYears = table.Column<int>(type: "integer", nullable: true),
                    PortfolioUrl = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewerProfiles_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewerAvailabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InterviewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsBooked = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewerAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewerAvailabilities_InterviewerProfiles_InterviewerId",
                        column: x => x.InterviewerId,
                        principalTable: "InterviewerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewerCompanies",
                columns: table => new
                {
                    InterviewerProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompaniesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewerCompanies", x => new { x.InterviewerProfilesId, x.CompaniesId });
                    table.ForeignKey(
                        name: "FK_InterviewerCompanies_Companies_CompaniesId",
                        column: x => x.CompaniesId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewerCompanies_InterviewerProfiles_InterviewerProfile~",
                        column: x => x.InterviewerProfilesId,
                        principalTable: "InterviewerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewerSkills",
                columns: table => new
                {
                    InterviewerProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewerSkills", x => new { x.InterviewerProfilesId, x.SkillsId });
                    table.ForeignKey(
                        name: "FK_InterviewerSkills_InterviewerProfiles_InterviewerProfilesId",
                        column: x => x.InterviewerProfilesId,
                        principalTable: "InterviewerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewerSkills_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Feedbacks",
                keyColumn: "Id",
                keyValue: new Guid("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10"),
                columns: new[] { "InterviewerId", "StudentId" },
                values: new object[] { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") });

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"),
                columns: new[] { "InterviewerId", "StudentId" },
                values: new object[] { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") });

            migrationBuilder.InsertData(
                table: "InterviewerProfiles",
                columns: new[] { "Id", "BankAccountNumber", "BankBinNumber", "Bio", "CurrentAmount", "ExperienceYears", "PortfolioUrl", "Status" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "", "", "Senior Backend Engineer with real interview experience", 0, 8, "https://portfolio.example.com/bob", 0 },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), "", "", "Fullstack Engineer previously at Uber", 0, 6, "https://portfolio.example.com/john", 0 },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), "", "", "Senior Frontend Engineer focusing on UI/UX interviews", 0, 7, "https://portfolio.example.com/sarah", 0 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"),
                columns: new[] { "FullName", "SlugProfileUrl" },
                values: new object[] { "Alice Student", "alice-student_1719000000001" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"),
                columns: new[] { "FullName", "SlugProfileUrl" },
                values: new object[] { "Bob Interviewer", "bob-interviewer_1719000000002" });

            migrationBuilder.InsertData(
                table: "InterviewerAvailabilities",
                columns: new[] { "Id", "EndTime", "InterviewerId", "IsBooked", "StartTime" },
                values: new object[] { new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), new DateTime(2025, 11, 1, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), false, new DateTime(2025, 11, 1, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "InterviewerCompanies",
                columns: new[] { "CompaniesId", "InterviewerProfilesId" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22") },
                    { new Guid("44444444-4444-4444-8444-444444444444"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22") },
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22") },
                    { new Guid("33333333-3333-4333-8333-333333333333"), new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44") },
                    { new Guid("66666666-6666-4666-8666-666666666666"), new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44") },
                    { new Guid("88888888-8888-4888-8888-888888888888"), new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44") },
                    { new Guid("22222222-2222-4222-8222-222222222222"), new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55") },
                    { new Guid("77777777-7777-4777-8777-777777777777"), new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55") },
                    { new Guid("99999999-9999-4999-8999-999999999999"), new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55") }
                });

            migrationBuilder.InsertData(
                table: "InterviewerSkills",
                columns: new[] { "InterviewerProfilesId", "SkillsId" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("02020202-0202-4202-8202-020202020202") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("06060606-0606-4606-8606-060606060606") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("07070707-0707-4707-8707-070707070707") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("08080808-0808-4808-8808-080808080808") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("04040404-0404-4404-8404-040404040404") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("07070707-0707-4707-8707-070707070707") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("09090909-0909-4909-8909-090909090909") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("0a0a0a0a-0a0a-4a0a-8a0a-0a0a0a0a0a0a") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("f5f5f5f5-f5f5-45f5-85f5-f5f5f5f5f5f5") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewerAvailabilities_InterviewerId",
                table: "InterviewerAvailabilities",
                column: "InterviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewerCompanies_CompaniesId",
                table: "InterviewerCompanies",
                column: "CompaniesId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewerSkills_SkillsId",
                table: "InterviewerSkills",
                column: "SkillsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_CandidateProfiles_StudentId",
                table: "Feedbacks",
                column: "StudentId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_InterviewerProfiles_InterviewerId",
                table: "Feedbacks",
                column: "InterviewerId",
                principalTable: "InterviewerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewRooms_CandidateProfiles_StudentId",
                table: "InterviewRooms",
                column: "StudentId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewRooms_InterviewerProfiles_InterviewerId",
                table: "InterviewRooms",
                column: "InterviewerId",
                principalTable: "InterviewerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

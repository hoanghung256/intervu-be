using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillAndCompanyManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Company",
                table: "InterviewerProfiles");

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterviewerCompanies",
                columns: table => new
                {
                    InterviewerProfilesId = table.Column<int>(type: "int", nullable: false),
                    CompaniesId = table.Column<int>(type: "int", nullable: false)
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
                        name: "FK_InterviewerCompanies_InterviewerProfiles_InterviewerProfilesId",
                        column: x => x.InterviewerProfilesId,
                        principalTable: "InterviewerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewerSkills",
                columns: table => new
                {
                    InterviewerProfilesId = table.Column<int>(type: "int", nullable: false),
                    SkillsId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "LogoPath", "Name", "Website" },
                values: new object[,]
                {
                    { 1, "logos/google.png", "Google", "https://google.com" },
                    { 2, "logos/meta.png", "Meta", "https://meta.com" },
                    { 3, "logos/amazon.png", "Amazon", "https://amazon.com" },
                    { 4, "logos/microsoft.png", "Microsoft", "https://microsoft.com" },
                    { 5, "logos/netflix.png", "Netflix", "https://netflix.com" },
                    { 6, "logos/tiktok.png", "TikTok", "https://tiktok.com" },
                    { 7, "logos/apple.png", "Apple", "https://apple.com" },
                    { 8, "logos/uber.png", "Uber", "https://uber.com" },
                    { 9, "logos/spotify.png", "Spotify", "https://spotify.com" },
                    { 10, "logos/stripe.png", "Stripe", "https://stripe.com" }
                });

            migrationBuilder.UpdateData(
                table: "InterviewerProfiles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Bio",
                value: "Senior Backend Engineer with real interview experience");

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, null, "C#" },
                    { 2, null, "Java" },
                    { 3, null, "JavaScript" },
                    { 4, null, "TypeScript" },
                    { 5, null, "React" },
                    { 6, null, "Node.js" },
                    { 7, null, "SQL" },
                    { 8, null, "MongoDB" },
                    { 9, null, "AWS" },
                    { 10, null, "Azure" },
                    { 11, null, "System Design" },
                    { 12, null, "Microservices" },
                    { 13, null, "Docker" },
                    { 14, null, "Kubernetes" },
                    { 15, null, "Machine Learning" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "Password", "ProfilePicture", "Role", "Status" },
                values: new object[,]
                {
                    { 5, "john.doe@example.com", "John Doe", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 1, 0 },
                    { 6, "sarah.lee@example.com", "Sarah Lee", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 1, 0 }
                });

            migrationBuilder.InsertData(
                table: "InterviewerCompanies",
                columns: new[] { "CompaniesId", "InterviewerProfilesId" },
                values: new object[,]
                {
                    { 1, 2 },
                    { 4, 2 },
                    { 10, 2 }
                });

            migrationBuilder.InsertData(
                table: "InterviewerProfiles",
                columns: new[] { "Id", "Bio", "CVUrl", "CurrentAmount", "ExperienceYears", "IsVerified", "PortfolioUrl", "ProgrammingLanguages", "Specializations" },
                values: new object[,]
                {
                    { 5, "Fullstack Engineer previously at Uber", "https://example.com/cv-john.pdf", 0, 6, true, "https://portfolio.example.com/john", "JavaScript, Go, TypeScript", "Fullstack, Cloud" },
                    { 6, "Senior Frontend Engineer focusing on UI/UX interviews", "https://example.com/cv-sarah.pdf", 0, 7, true, "https://portfolio.example.com/sarah", "JavaScript, TypeScript", "Frontend, UI/UX" }
                });

            migrationBuilder.InsertData(
                table: "InterviewerSkills",
                columns: new[] { "InterviewerProfilesId", "SkillsId" },
                values: new object[,]
                {
                    { 2, 1 },
                    { 2, 7 },
                    { 2, 11 },
                    { 2, 12 },
                    { 2, 13 }
                });

            migrationBuilder.InsertData(
                table: "InterviewerCompanies",
                columns: new[] { "CompaniesId", "InterviewerProfilesId" },
                values: new object[,]
                {
                    { 3, 5 },
                    { 6, 5 },
                    { 8, 5 },
                    { 2, 6 },
                    { 7, 6 },
                    { 9, 6 }
                });

            migrationBuilder.InsertData(
                table: "InterviewerSkills",
                columns: new[] { "InterviewerProfilesId", "SkillsId" },
                values: new object[,]
                {
                    { 5, 3 },
                    { 5, 4 },
                    { 5, 9 },
                    { 5, 12 },
                    { 5, 14 },
                    { 6, 3 },
                    { 6, 4 },
                    { 6, 5 },
                    { 6, 15 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewerCompanies_CompaniesId",
                table: "InterviewerCompanies",
                column: "CompaniesId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewerSkills_SkillsId",
                table: "InterviewerSkills",
                column: "SkillsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewerCompanies");

            migrationBuilder.DropTable(
                name: "InterviewerSkills");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DeleteData(
                table: "InterviewerProfiles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "InterviewerProfiles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "InterviewerProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "InterviewerProfiles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Bio", "Company" },
                values: new object[] { "Senior software engineer", "Tech Co" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "hashedpassword");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "hashedpassword");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "hashedpassword");
        }
    }
}

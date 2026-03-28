using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddIndustryAndCoachTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentJobTitle",
                table: "CoachProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Industries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Industries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoachIndustries",
                columns: table => new
                {
                    CoachProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndustriesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachIndustries", x => new { x.CoachProfilesId, x.IndustriesId });
                    table.ForeignKey(
                        name: "FK_CoachIndustries_CoachProfiles_CoachProfilesId",
                        column: x => x.CoachProfilesId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachIndustries_Industries_IndustriesId",
                        column: x => x.IndustriesId,
                        principalTable: "Industries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "CoachProfiles",
                keyColumn: "Id",
                keyValue: new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"),
                column: "CurrentJobTitle",
                value: "Senior Backend Engineer");

            migrationBuilder.UpdateData(
                table: "CoachProfiles",
                keyColumn: "Id",
                keyValue: new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"),
                column: "CurrentJobTitle",
                value: "Technical Lead");

            migrationBuilder.UpdateData(
                table: "CoachProfiles",
                keyColumn: "Id",
                keyValue: new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"),
                column: "CurrentJobTitle",
                value: "Senior Frontend Engineer");

            migrationBuilder.InsertData(
                table: "Industries",
                columns: new[] { "Id", "Name", "Slug" },
                values: new object[,]
                {
                    { new Guid("11110000-0000-4000-8000-000000000001"), "Fintech", "fintech" },
                    { new Guid("11110000-0000-4000-8000-000000000002"), "E-commerce", "e-commerce" },
                    { new Guid("11110000-0000-4000-8000-000000000003"), "EdTech", "edtech" },
                    { new Guid("11110000-0000-4000-8000-000000000004"), "Blockchain", "blockchain" },
                    { new Guid("11110000-0000-4000-8000-000000000005"), "HealthTech", "healthtech" },
                    { new Guid("11110000-0000-4000-8000-000000000006"), "SaaS", "saas" },
                    { new Guid("11110000-0000-4000-8000-000000000007"), "AI/ML", "ai-ml" },
                    { new Guid("11110000-0000-4000-8000-000000000008"), "GameDev", "gamedev" }
                });

            migrationBuilder.InsertData(
                table: "CoachIndustries",
                columns: new[] { "CoachProfilesId", "IndustriesId" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("11110000-0000-4000-8000-000000000001") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("11110000-0000-4000-8000-000000000006") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("11110000-0000-4000-8000-000000000002") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("11110000-0000-4000-8000-000000000007") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("11110000-0000-4000-8000-000000000003") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("11110000-0000-4000-8000-000000000008") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoachIndustries_IndustriesId",
                table: "CoachIndustries",
                column: "IndustriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Industries_Name",
                table: "Industries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Industries_Slug",
                table: "Industries",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoachIndustries");

            migrationBuilder.DropTable(
                name: "Industries");

            migrationBuilder.DropColumn(
                name: "CurrentJobTitle",
                table: "CoachProfiles");
        }
    }
}

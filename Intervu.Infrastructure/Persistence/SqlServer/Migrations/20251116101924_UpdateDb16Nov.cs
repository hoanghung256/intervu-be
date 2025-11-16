using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDb16Nov : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVUrl",
                table: "InterviewerProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "ExperienceYears",
                table: "InterviewerProfiles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentAmount",
                table: "InterviewerProfiles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Skills",
                table: "IntervieweeProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PortfolioUrl",
                table: "IntervieweeProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "CVUrl",
                table: "IntervieweeProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "IntervieweeProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "InterviewerProfiles",
                keyColumn: "Id",
                keyValue: 5,
                column: "Status",
                value: 0);

            migrationBuilder.UpdateData(
                table: "InterviewerProfiles",
                keyColumn: "Id",
                keyValue: 6,
                column: "Status",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ExperienceYears",
                table: "InterviewerProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentAmount",
                table: "InterviewerProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVUrl",
                table: "InterviewerProfiles",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Skills",
                table: "IntervieweeProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PortfolioUrl",
                table: "IntervieweeProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CVUrl",
                table: "IntervieweeProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "IntervieweeProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "InterviewerProfiles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CVUrl",
                value: "https://example.com/cv-bob.pdf");

            migrationBuilder.UpdateData(
                table: "InterviewerProfiles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CVUrl", "Status" },
                values: new object[] { "https://example.com/cv-john.pdf", 1 });

            migrationBuilder.UpdateData(
                table: "InterviewerProfiles",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CVUrl", "Status" },
                values: new object[] { "https://example.com/cv-sarah.pdf", 1 });
        }
    }
}

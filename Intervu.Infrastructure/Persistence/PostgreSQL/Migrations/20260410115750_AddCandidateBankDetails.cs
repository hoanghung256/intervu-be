using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateBankDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "CandidateProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankBinNumber",
                table: "CandidateProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "CandidateProfiles",
                keyColumn: "Id",
                keyValue: new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"),
                columns: new[] { "BankAccountNumber", "BankBinNumber" },
                values: new object[] { "", "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "BankBinNumber",
                table: "CandidateProfiles");
        }
    }
}

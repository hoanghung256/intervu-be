using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddEncriptBankAccoutnField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BankAccountNumber",
                table: "WithdrawalRequests",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumberMasked",
                table: "WithdrawalRequests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumberMasked",
                table: "CoachProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumberMasked",
                table: "CandidateProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "CandidateProfiles",
                keyColumn: "Id",
                keyValue: new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"),
                column: "BankAccountNumberMasked",
                value: "");

            migrationBuilder.UpdateData(
                table: "CoachProfiles",
                keyColumn: "Id",
                keyValue: new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"),
                column: "BankAccountNumberMasked",
                value: "");

            migrationBuilder.UpdateData(
                table: "CoachProfiles",
                keyColumn: "Id",
                keyValue: new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"),
                column: "BankAccountNumberMasked",
                value: "");

            migrationBuilder.UpdateData(
                table: "CoachProfiles",
                keyColumn: "Id",
                keyValue: new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"),
                column: "BankAccountNumberMasked",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountNumberMasked",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "BankAccountNumberMasked",
                table: "CoachProfiles");

            migrationBuilder.DropColumn(
                name: "BankAccountNumberMasked",
                table: "CandidateProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "BankAccountNumber",
                table: "WithdrawalRequests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewRoundIdToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InterviewRoundId",
                table: "InterviewBookingTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a00"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a11"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a22"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a99"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5e"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("c1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5f"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("f1f1f1f1-9999-4a1a-8a1a-999999999999"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewBookingTransaction_InterviewRoundId_Type",
                table: "InterviewBookingTransaction",
                columns: new[] { "InterviewRoundId", "Type" });

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewBookingTransaction_InterviewRounds_InterviewRoundId",
                table: "InterviewBookingTransaction",
                column: "InterviewRoundId",
                principalTable: "InterviewRounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterviewBookingTransaction_InterviewRounds_InterviewRoundId",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropIndex(
                name: "IX_InterviewBookingTransaction_InterviewRoundId_Type",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropColumn(
                name: "InterviewRoundId",
                table: "InterviewBookingTransaction");
        }
    }
}

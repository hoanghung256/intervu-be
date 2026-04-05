using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddTransAndQuestList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuestionList",
                table: "InterviewRooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Transcript",
                table: "InterviewRooms",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"),
                columns: new[] { "QuestionList", "Transcript" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a77"),
                columns: new[] { "QuestionList", "Transcript" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a88"),
                columns: new[] { "QuestionList", "Transcript" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionList",
                table: "InterviewRooms");

            migrationBuilder.DropColumn(
                name: "Transcript",
                table: "InterviewRooms");
        }
    }
}

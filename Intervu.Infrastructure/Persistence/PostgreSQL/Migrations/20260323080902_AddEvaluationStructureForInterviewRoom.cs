using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluationStructureForInterviewRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvaluationStructure",
                table: "InterviewRooms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEvaluationCompleted",
                table: "InterviewRooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"),
                columns: new[] { "EvaluationStructure", "IsEvaluationCompleted" },
                values: new object[] { null, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvaluationStructure",
                table: "InterviewRooms");

            migrationBuilder.DropColumn(
                name: "IsEvaluationCompleted",
                table: "InterviewRooms");
        }
    }
}

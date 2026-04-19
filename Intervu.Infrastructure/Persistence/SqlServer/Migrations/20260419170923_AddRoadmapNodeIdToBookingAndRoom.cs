using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadmapNodeIdToBookingAndRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoadmapNodeId",
                table: "InterviewRooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoadmapNodeId",
                table: "BookingRequests",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"),
                column: "RoadmapNodeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a77"),
                column: "RoadmapNodeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a88"),
                column: "RoadmapNodeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
                column: "RoadmapNodeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("b1b1b1b1-2222-4a1a-8a1a-222222222222"),
                column: "RoadmapNodeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("c1c1c1c1-3333-4a1a-8a1a-333333333333"),
                column: "RoadmapNodeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                column: "RoadmapNodeId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoadmapNodeId",
                table: "InterviewRooms");

            migrationBuilder.DropColumn(
                name: "RoadmapNodeId",
                table: "BookingRequests");
        }
    }
}

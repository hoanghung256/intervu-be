using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewRoomType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "InterviewRooms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AudioChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AudioData = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    RecordingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkSequenceNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioChunks", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CoachAvailabilities",
                columns: new[] { "Id", "CoachId", "EndTime", "StartTime", "Status" },
                values: new object[] { new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111112"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 4, 1, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 1, 10, 0, 0, 0, DateTimeKind.Utc), 0 });

            migrationBuilder.InsertData(
                table: "InterviewBookingTransaction",
                columns: new[] { "Id", "Amount", "BookedDurationMinutes", "BookedStartTime", "BookingRequestId", "CoachAvailabilityId", "CoachId", "Status", "Type", "UserId" },
                values: new object[] { new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a99"), 1500, null, null, null, new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111111"), null, 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") });

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"),
                column: "Type",
                value: 0);

            migrationBuilder.InsertData(
                table: "InterviewBookingTransaction",
                columns: new[] { "Id", "Amount", "BookedDurationMinutes", "BookedStartTime", "BookingRequestId", "CoachAvailabilityId", "CoachId", "Status", "Type", "UserId" },
                values: new object[] { new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a00"), 2000, null, null, null, new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111112"), null, 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") });

            migrationBuilder.InsertData(
                table: "InterviewRooms",
                columns: new[] { "Id", "AimLevel", "BookingRequestId", "CandidateId", "CoachId", "CoachInterviewServiceId", "CurrentAvailabilityId", "CurrentLanguage", "DurationMinutes", "EvaluationStructure", "IsEvaluationCompleted", "LanguageCodes", "ProblemDescription", "ProblemShortName", "RescheduleAttemptCount", "RoundNumber", "ScheduledTime", "Status", "TestCases", "TransactionId", "Type", "VideoCallRoomUrl" },
                values: new object[,]
                {
                    { new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a77"), null, null, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), null, null, new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111111"), null, 60, null, false, null, null, null, 0, null, new DateTime(2026, 3, 15, 14, 30, 0, 0, DateTimeKind.Utc), 1, null, new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a99"), 1, "https://meet.example/room-ai" },
                    { new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a88"), null, null, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111112"), null, 60, null, false, null, null, null, 0, null, new DateTime(2026, 4, 1, 10, 0, 0, 0, DateTimeKind.Utc), 1, null, new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a00"), 0, "https://meet.example/room3" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AudioChunks_RecordingSession_Sequence",
                table: "AudioChunks",
                columns: new[] { "RecordingSessionId", "ChunkSequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AudioChunks_RecordingSessionId",
                table: "AudioChunks",
                column: "RecordingSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudioChunks");

            migrationBuilder.DeleteData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a77"));

            migrationBuilder.DeleteData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a88"));

            migrationBuilder.DeleteData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a00"));

            migrationBuilder.DeleteData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a99"));

            migrationBuilder.DeleteData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111112"));

            migrationBuilder.DropColumn(
                name: "Type",
                table: "InterviewRooms");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CancelInMultipleBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "InterviewRounds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "CoachAvailabilities",
                columns: new[] { "Id", "CoachId", "EndTime", "InterviewRoundId", "StartTime", "Status" },
                values: new object[,]
                {
                    { new Guid("d1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c60"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 5, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 5, 1, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { new Guid("d1d1d1d1-4444-4a1a-8a1a-444444444444"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 5, 3, 10, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 5, 3, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { new Guid("e1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c61"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 5, 2, 10, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 5, 2, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { new Guid("e1e1e1e1-5555-4a1a-8a1a-555555555555"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 5, 4, 10, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 5, 4, 9, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { new Guid("f1f1f1f1-8888-4a1a-8a1a-888888888888"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 5, 5, 10, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 5, 5, 9, 0, 0, 0, DateTimeKind.Utc), 0 }
                });

            migrationBuilder.InsertData(
                table: "InterviewBookingTransaction",
                columns: new[] { "Id", "Amount", "BookingRequestId", "Status", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a11"), 2000, null, 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a22"), 2000, null, 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("b1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5e"), 2000, null, 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("c1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5f"), 2000, null, 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("f1f1f1f1-9999-4a1a-8a1a-999999999999"), 2000, null, 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") }
                });

            migrationBuilder.InsertData(
                table: "InterviewRooms",
                columns: new[] { "Id", "AimLevel", "BookingRequestId", "CandidateId", "CoachId", "CoachInterviewServiceId", "CurrentAvailabilityId", "CurrentLanguage", "DurationMinutes", "EvaluationStructure", "IsEvaluationCompleted", "LanguageCodes", "ProblemDescription", "ProblemShortName", "QuestionList", "RescheduleAttemptCount", "RoundNumber", "ScheduledTime", "Status", "TestCases", "TransactionId", "Transcript", "Type", "VideoCallRoomUrl" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"), null, null, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, new Guid("e1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c61"), null, 60, null, false, null, null, null, null, 0, null, new DateTime(2026, 5, 2, 9, 0, 0, 0, DateTimeKind.Utc), 2, null, new Guid("c1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5f"), null, 0, "https://meet.example/room-report" },
                    { new Guid("b1b1b1b1-2222-4a1a-8a1a-222222222222"), null, null, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), null, 60, null, false, null, null, null, null, 0, null, new DateTime(2026, 5, 10, 9, 0, 0, 0, DateTimeKind.Utc), 0, null, new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a11"), null, 0, "https://meet.example/room-resch-create" },
                    { new Guid("c1c1c1c1-3333-4a1a-8a1a-333333333333"), null, null, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), null, 60, null, false, null, null, null, null, 0, null, new DateTime(2026, 5, 12, 9, 0, 0, 0, DateTimeKind.Utc), 0, null, new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a22"), null, 0, "https://meet.example/room-resch-respond" },
                    { new Guid("f1f1f1f1-7777-4a1a-8a1a-777777777777"), null, null, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, new Guid("f1f1f1f1-8888-4a1a-8a1a-888888888888"), null, 60, null, false, null, null, null, null, 0, null, new DateTime(2026, 5, 5, 9, 0, 0, 0, DateTimeKind.Utc), 2, null, new Guid("f1f1f1f1-9999-4a1a-8a1a-999999999999"), null, 0, "https://meet.example/room-feedback" },
                    { new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"), null, null, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, new Guid("d1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c60"), null, 60, null, false, null, null, null, null, 0, null, new DateTime(2026, 5, 1, 9, 0, 0, 0, DateTimeKind.Utc), 1, null, new Guid("b1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5e"), null, 0, "https://meet.example/room-eval" }
                });

            migrationBuilder.InsertData(
                table: "Feedbacks",
                columns: new[] { "Id", "AIAnalysis", "CandidateId", "CoachId", "Comments", "InterviewRoomId", "Rating" },
                values: new object[] { new Guid("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c11"), "{}", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "", new Guid("f1f1f1f1-7777-4a1a-8a1a-777777777777"), 0 });

            migrationBuilder.InsertData(
                table: "InterviewRescheduleRequests",
                columns: new[] { "Id", "CurrentAvailabilityId", "ExpiresAt", "InterviewRoomId", "ProposedAvailabilityId", "ProposedEndTime", "ProposedStartTime", "Reason", "RejectionReason", "RequestedBy", "RespondedAt", "RespondedBy", "Status" },
                values: new object[] { new Guid("f1f1f1f1-6666-4a1a-8a1a-666666666666"), new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c1c1c1c1-3333-4a1a-8a1a-333333333333"), new Guid("e1e1e1e1-5555-4a1a-8a1a-555555555555"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Seed reason for testing response", null, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), null, null, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("d1d1d1d1-4444-4a1a-8a1a-444444444444"));

            migrationBuilder.DeleteData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("f1f1f1f1-8888-4a1a-8a1a-888888888888"));

            migrationBuilder.DeleteData(
                table: "Feedbacks",
                keyColumn: "Id",
                keyValue: new Guid("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c11"));

            migrationBuilder.DeleteData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("f1f1f1f1-9999-4a1a-8a1a-999999999999"));

            migrationBuilder.DeleteData(
                table: "InterviewRescheduleRequests",
                keyColumn: "Id",
                keyValue: new Guid("f1f1f1f1-6666-4a1a-8a1a-666666666666"));

            migrationBuilder.DeleteData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"));

            migrationBuilder.DeleteData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("b1b1b1b1-2222-4a1a-8a1a-222222222222"));

            migrationBuilder.DeleteData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("f1f1f1f1-7777-4a1a-8a1a-777777777777"));

            migrationBuilder.DeleteData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"));

            migrationBuilder.DeleteData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("d1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c60"));

            migrationBuilder.DeleteData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c61"));

            migrationBuilder.DeleteData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e1e1e1e1-5555-4a1a-8a1a-555555555555"));

            migrationBuilder.DeleteData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a11"));

            migrationBuilder.DeleteData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5e"));

            migrationBuilder.DeleteData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("c1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5f"));

            migrationBuilder.DeleteData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("c1c1c1c1-3333-4a1a-8a1a-333333333333"));

            migrationBuilder.DeleteData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a22"));

            migrationBuilder.DropColumn(
                name: "Status",
                table: "InterviewRounds");
        }
    }
}

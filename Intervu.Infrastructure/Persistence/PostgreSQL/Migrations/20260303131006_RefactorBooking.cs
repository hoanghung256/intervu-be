using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachAvailabilities_InterviewTypes_TypeId",
                table: "CoachAvailabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_CoachAvailabilities_Users_ReservingForUserId",
                table: "CoachAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_CoachAvailabilities_ReservingForUserId",
                table: "CoachAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_CoachAvailabilities_TypeId",
                table: "CoachAvailabilities");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "InterviewTypes");

            migrationBuilder.DropColumn(
                name: "Focus",
                table: "CoachAvailabilities");

            migrationBuilder.DropColumn(
                name: "ReservingForUserId",
                table: "CoachAvailabilities");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "CoachAvailabilities");

            migrationBuilder.RenameColumn(
                name: "BasePrice",
                table: "InterviewTypes",
                newName: "MinPrice");

            migrationBuilder.AddColumn<int>(
                name: "MaxPrice",
                table: "InterviewTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SuggestedDurationMinutes",
                table: "InterviewTypes",
                type: "integer",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentAvailabilityId",
                table: "InterviewRooms",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "AimLevel",
                table: "InterviewRooms",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BookingRequestId",
                table: "InterviewRooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CoachInterviewServiceId",
                table: "InterviewRooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoundNumber",
                table: "InterviewRooms",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CoachAvailabilityId",
                table: "InterviewBookingTransaction",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "BookingRequestId",
                table: "InterviewBookingTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CoachInterviewServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachInterviewServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachInterviewServices_CoachProfiles_CoachId",
                        column: x => x.CoachId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachInterviewServices_InterviewTypes_InterviewTypeId",
                        column: x => x.InterviewTypeId,
                        principalTable: "InterviewTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CoachInterviewServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AimLevel = table.Column<int>(type: "integer", nullable: true),
                    JobDescriptionUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CVUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalAmount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingRequests_CandidateProfiles_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingRequests_CoachInterviewServices_CoachInterviewServiceId",
                        column: x => x.CoachInterviewServiceId,
                        principalTable: "CoachInterviewServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingRequests_CoachProfiles_CoachId",
                        column: x => x.CoachId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InterviewRounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachInterviewServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundNumber = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    InterviewRoomId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewRounds_BookingRequests_BookingRequestId",
                        column: x => x.BookingRequestId,
                        principalTable: "BookingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewRounds_CoachInterviewServices_CoachInterviewServiceId",
                        column: x => x.CoachInterviewServiceId,
                        principalTable: "CoachInterviewServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRounds_InterviewRooms_InterviewRoomId",
                        column: x => x.InterviewRoomId,
                        principalTable: "InterviewRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"),
                columns: new[] { "EndTime", "Status" },
                values: new object[] { new DateTime(2026, 2, 10, 12, 0, 0, 0, DateTimeKind.Utc), 0 });

            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111111"),
                column: "EndTime",
                value: new DateTime(2026, 3, 15, 17, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"),
                column: "BookingRequestId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99"),
                column: "BookingRequestId",
                value: null);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"),
                columns: new[] { "AimLevel", "BookingRequestId", "CoachInterviewServiceId", "RoundNumber" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"),
                columns: new[] { "MaxPrice", "MinPrice", "SuggestedDurationMinutes" },
                values: new object[] { 60, 15, 45 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"),
                columns: new[] { "MaxPrice", "MinPrice", "SuggestedDurationMinutes" },
                values: new object[] { 50, 10, 30 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"),
                columns: new[] { "MaxPrice", "MinPrice", "SuggestedDurationMinutes" },
                values: new object[] { 100, 30, 60 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"),
                columns: new[] { "MaxPrice", "MinPrice", "SuggestedDurationMinutes" },
                values: new object[] { 120, 40, 75 });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_BookingRequestId",
                table: "InterviewRooms",
                column: "BookingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_CoachInterviewServiceId",
                table: "InterviewRooms",
                column: "CoachInterviewServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewBookingTransaction_BookingRequestId",
                table: "InterviewBookingTransaction",
                column: "BookingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_CandidateId",
                table: "BookingRequests",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_CoachId",
                table: "BookingRequests",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_CoachInterviewServiceId",
                table: "BookingRequests",
                column: "CoachInterviewServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_ExpiresAt",
                table: "BookingRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_Status",
                table: "BookingRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CoachInterviewServices_CoachId_InterviewTypeId",
                table: "CoachInterviewServices",
                columns: new[] { "CoachId", "InterviewTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoachInterviewServices_InterviewTypeId",
                table: "CoachInterviewServices",
                column: "InterviewTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRounds_BookingRequestId_RoundNumber",
                table: "InterviewRounds",
                columns: new[] { "BookingRequestId", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRounds_CoachInterviewServiceId",
                table: "InterviewRounds",
                column: "CoachInterviewServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRounds_InterviewRoomId",
                table: "InterviewRounds",
                column: "InterviewRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewBookingTransaction_BookingRequests_BookingRequestId",
                table: "InterviewBookingTransaction",
                column: "BookingRequestId",
                principalTable: "BookingRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewRooms_BookingRequests_BookingRequestId",
                table: "InterviewRooms",
                column: "BookingRequestId",
                principalTable: "BookingRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewRooms_CoachInterviewServices_CoachInterviewServiceId",
                table: "InterviewRooms",
                column: "CoachInterviewServiceId",
                principalTable: "CoachInterviewServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterviewBookingTransaction_BookingRequests_BookingRequestId",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewRooms_BookingRequests_BookingRequestId",
                table: "InterviewRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewRooms_CoachInterviewServices_CoachInterviewServiceId",
                table: "InterviewRooms");

            migrationBuilder.DropTable(
                name: "InterviewRounds");

            migrationBuilder.DropTable(
                name: "BookingRequests");

            migrationBuilder.DropTable(
                name: "CoachInterviewServices");

            migrationBuilder.DropIndex(
                name: "IX_InterviewRooms_BookingRequestId",
                table: "InterviewRooms");

            migrationBuilder.DropIndex(
                name: "IX_InterviewRooms_CoachInterviewServiceId",
                table: "InterviewRooms");

            migrationBuilder.DropIndex(
                name: "IX_InterviewBookingTransaction_BookingRequestId",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropColumn(
                name: "MaxPrice",
                table: "InterviewTypes");

            migrationBuilder.DropColumn(
                name: "SuggestedDurationMinutes",
                table: "InterviewTypes");

            migrationBuilder.DropColumn(
                name: "AimLevel",
                table: "InterviewRooms");

            migrationBuilder.DropColumn(
                name: "BookingRequestId",
                table: "InterviewRooms");

            migrationBuilder.DropColumn(
                name: "CoachInterviewServiceId",
                table: "InterviewRooms");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "InterviewRooms");

            migrationBuilder.DropColumn(
                name: "BookingRequestId",
                table: "InterviewBookingTransaction");

            migrationBuilder.RenameColumn(
                name: "MinPrice",
                table: "InterviewTypes",
                newName: "BasePrice");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "InterviewTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentAvailabilityId",
                table: "InterviewRooms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CoachAvailabilityId",
                table: "InterviewBookingTransaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Focus",
                table: "CoachAvailabilities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ReservingForUserId",
                table: "CoachAvailabilities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TypeId",
                table: "CoachAvailabilities",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"),
                columns: new[] { "EndTime", "Focus", "ReservingForUserId", "Status", "TypeId" },
                values: new object[] { new DateTime(2026, 2, 10, 10, 0, 0, 0, DateTimeKind.Utc), 0, null, 2, null });

            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111111"),
                columns: new[] { "EndTime", "Focus", "ReservingForUserId", "TypeId" },
                values: new object[] { new DateTime(2026, 3, 15, 15, 0, 0, 0, DateTimeKind.Utc), 0, null, null });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"),
                columns: new[] { "BasePrice", "DurationMinutes" },
                values: new object[] { 30, 45 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"),
                columns: new[] { "BasePrice", "DurationMinutes" },
                values: new object[] { 20, 30 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"),
                columns: new[] { "BasePrice", "DurationMinutes" },
                values: new object[] { 50, 60 });

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"),
                columns: new[] { "BasePrice", "DurationMinutes" },
                values: new object[] { 70, 75 });

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_ReservingForUserId",
                table: "CoachAvailabilities",
                column: "ReservingForUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_TypeId",
                table: "CoachAvailabilities",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoachAvailabilities_InterviewTypes_TypeId",
                table: "CoachAvailabilities",
                column: "TypeId",
                principalTable: "InterviewTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CoachAvailabilities_Users_ReservingForUserId",
                table: "CoachAvailabilities",
                column: "ReservingForUserId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

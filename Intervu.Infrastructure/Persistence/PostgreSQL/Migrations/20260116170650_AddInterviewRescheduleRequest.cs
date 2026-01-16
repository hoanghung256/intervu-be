using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewRescheduleRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewRescheduleRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewBookingTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentAvailabilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposedAvailabilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RespondedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewRescheduleRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewRescheduleRequests_CoachAvailabilities_CurrentAvailabilityId",
                        column: x => x.CurrentAvailabilityId,
                        principalTable: "CoachAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRescheduleRequests_CoachAvailabilities_ProposedAvailabilityId",
                        column: x => x.ProposedAvailabilityId,
                        principalTable: "CoachAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRescheduleRequests_InterviewBookingTransaction_InterviewBookingTransactionId",
                        column: x => x.InterviewBookingTransactionId,
                        principalTable: "InterviewBookingTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRescheduleRequests_Users_RequestedBy",
                        column: x => x.RequestedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRescheduleRequests_Users_RespondedBy",
                        column: x => x.RespondedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_CurrentAvailabilityId",
                table: "InterviewRescheduleRequests",
                column: "CurrentAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_ExpiresAt",
                table: "InterviewRescheduleRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_InterviewBookingTransactionId",
                table: "InterviewRescheduleRequests",
                column: "InterviewBookingTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_InterviewBookingTransactionId_S~",
                table: "InterviewRescheduleRequests",
                columns: new[] { "InterviewBookingTransactionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_ProposedAvailabilityId",
                table: "InterviewRescheduleRequests",
                column: "ProposedAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_RequestedBy",
                table: "InterviewRescheduleRequests",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_RespondedBy",
                table: "InterviewRescheduleRequests",
                column: "RespondedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRescheduleRequests_Status",
                table: "InterviewRescheduleRequests",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewRescheduleRequests");
        }
    }
}

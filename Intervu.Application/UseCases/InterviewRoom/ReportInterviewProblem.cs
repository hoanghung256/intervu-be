using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class ReportInterviewProblem(
        IUnitOfWork unitOfWork,
        IBackgroundService jobService) : IReportInterviewProblem
    {
        private const int MaxReasonLength = 500;
        private const int MaxDetailsLength = 4000;

        public async Task<CreateRoomReportResult> ExecuteAsync(Guid interviewRoomId, CreateRoomReportRequest request, Guid userId)
        {
            if (request == null)
            {
                throw new BadRequestException("Request body is required");
            }

            var reason = request?.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new BadRequestException("Report reason is required");
            }

            if (reason.Length > MaxReasonLength)
            {
                throw new BadRequestException($"Report reason must be less than or equal to {MaxReasonLength} characters");
            }

            var details = request?.Details?.Trim();
            if (!string.IsNullOrEmpty(details) && details.Length > MaxDetailsLength)
            {
                throw new BadRequestException($"Report details must be less than or equal to {MaxDetailsLength} characters");
            }

            var roomRepository = unitOfWork.GetRepository<IInterviewRoomRepository>();
            var reportRepository = unitOfWork.GetRepository<IInterviewReportRepository>();

            var room = await roomRepository.GetByIdWithDetailsAsync(interviewRoomId);
            if (room == null)
            {
                throw new NotFoundException("Interview room not found");
            }

            // Check if room has already been reported
            if (await reportRepository.ExistsByRoomIdAsync(interviewRoomId))
            {
                throw new BadRequestException("This interview room has already been reported");
            }

            if (room.CandidateId != userId)
            {
                throw new ForbiddenException("Only the candidate in this room can submit a report");
            }

            var now = DateTime.UtcNow;
            var report = new InterviewReport
            {
                Id = Guid.NewGuid(),
                InterviewRoomId = interviewRoomId,
                ReportedBy = userId,
                ReporterId = userId,
                Reason = reason,
                Details = details ?? string.Empty,
                Status = InterviewReportStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now
            };

            // Mark transaction as PendingPayout if it exists to freeze coach payment
            if (room.Transaction != null)
            {
                room.Transaction.Status = TransactionStatus.PendingPayout;
                unitOfWork.GetRepository<ITransactionRepository>().UpdateAsync(room.Transaction);
            }

            await reportRepository.AddAsync(report);
            await unitOfWork.SaveChangesAsync();

            // Send notification to candidate
            jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                userId,
                NotificationType.SystemAnnouncement,
                "Report Submitted",
                $"Your report for interview room {interviewRoomId.ToString().Substring(0, 8)} has been submitted successfully and is being reviewed by our team.",
                "/history",
                null
            ));

            // TODO: Send email to candidate confirming report receipt


            return new CreateRoomReportResult
            {
                ReportId = report.Id
            };
        }
    }
}

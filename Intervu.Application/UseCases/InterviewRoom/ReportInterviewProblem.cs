using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Utils;
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
        IBackgroundService jobService,
        IUserRepository userRepository) : IReportInterviewProblem
    {
        private const int MaxReasonLength = 500;
        private const int MaxDetailsLength = 4000;
        private const int MaxExpectToLength = 1000;

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

            var expectTo = request?.ExpectTo?.Trim();
            if (!string.IsNullOrEmpty(expectTo) && expectTo.Length > MaxExpectToLength)
            {
                throw new BadRequestException($"Report expectation must be less than or equal to {MaxExpectToLength} characters");
            }

            var roomRepository = unitOfWork.GetRepository<IInterviewRoomRepository>();
            var reportRepository = unitOfWork.GetRepository<IInterviewReportRepository>();

            var room = await roomRepository.GetByIdWithDetailsAsync(interviewRoomId);
            if (room == null)
            {
                throw new NotFoundException("Interview room not found");
            }

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
                ExpectTo = string.IsNullOrEmpty(expectTo) ? null : expectTo,
                Status = InterviewReportStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now
            };

            await unitOfWork.BeginTransactionAsync();
            try
            {
                await reportRepository.AddAsync(report);

                // Race handler: if PayoutForCoachAfterInterview already credited the coach for this round,
                // reverse the credit and flip the payout to PendingPayout so admin resolution can decide.
                var roundRepo = unitOfWork.GetRepository<IInterviewRoundRepository>();
                var transactionRepo = unitOfWork.GetRepository<ITransactionRepository>();
                var coachProfileRepo = unitOfWork.GetRepository<ICoachProfileRepository>();

                var round = await roundRepo.GetByInterviewRoomIdAsync(interviewRoomId);
                if (round != null && room.CoachId.HasValue)
                {
                    var payout = await transactionRepo.GetByInterviewRoundId(round.Id, TransactionType.Payout);
                    if (payout != null && payout.Status == TransactionStatus.Paid)
                    {
                        var coach = await coachProfileRepo.GetProfileByIdAsync(room.CoachId.Value);
                        if (coach != null)
                        {
                            coach.CurrentAmount = (coach.CurrentAmount ?? 0) - payout.Amount;
                            coach.Version++;
                            await coachProfileRepo.UpdateCoachProfileAsync(coach);

                            await transactionRepo.AddAsync(new InterviewBookingTransaction
                            {
                                Id = Guid.NewGuid(),
                                OrderCode = RandomGenerator.GenerateOrderCode(),
                                UserId = room.CoachId.Value,
                                BookingRequestId = payout.BookingRequestId,
                                InterviewRoundId = round.Id,
                                Amount = -payout.Amount,
                                GrossAmount = payout.GrossAmount.HasValue ? -payout.GrossAmount.Value : null,
                                CommissionAmount = payout.CommissionAmount.HasValue ? -payout.CommissionAmount.Value : null,
                                CommissionRate = payout.CommissionRate,
                                Type = TransactionType.Earnings,
                                Status = TransactionStatus.Paid,
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        payout.Status = TransactionStatus.PendingPayout;
                        transactionRepo.UpdateAsync(payout);

                        var coachId = room.CoachId.Value;
                        var roomShortId = interviewRoomId.ToString().Substring(0, 8).ToUpperInvariant();
                        var holdMessage = $"Your payout for interview room {roomShortId} has been placed on hold pending review of a candidate report.";
                        jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                            coachId,
                            NotificationType.SystemAnnouncement,
                            "Payout On Hold",
                            holdMessage,
                            "/payment-history",
                            null
                        ));
                    }
                }

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }

            jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                userId,
                NotificationType.SystemAnnouncement,
                "Report Submitted",
                $"Your report for interview room {interviewRoomId.ToString().Substring(0, 8)} has been submitted successfully and is being reviewed by our team.",
                null,
                report.Id
            ));

            var reporter = await userRepository.GetByIdAsync(userId);
            if (reporter != null)
            {
                try
                {
                    var placeholders = new Dictionary<string, string>
                    {
                        ["RecipientName"] = reporter.FullName,
                        ["RoomId"] = interviewRoomId.ToString()[..8].ToUpperInvariant(),
                        ["Reason"] = reason
                    };

                    jobService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                        reporter.Email,
                        "ReportReceipt",
                        placeholders));
                }
                catch
                {
                    // Do not fail report submission if email enqueue fails.
                }
            }


            return new CreateRoomReportResult
            {
                ReportId = report.Id
            };
        }
    }
}

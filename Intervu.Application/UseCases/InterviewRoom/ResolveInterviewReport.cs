using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Utils;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class ResolveInterviewReport(
        IUnitOfWork unitOfWork,
        IBackgroundService jobService,
        IUserRepository userRepository) : IResolveInterviewReport
    {
        public async Task ExecuteAsync(ResolveRoomReportRequest request, Guid adminId)
        {
            if (request == null) throw new BadRequestException("Request is required");

            await unitOfWork.BeginTransactionAsync();
            try
            {
                var reportRepo = unitOfWork.GetRepository<IInterviewReportRepository>();
                var roomRepo = unitOfWork.GetRepository<IInterviewRoomRepository>();
                var transactionRepo = unitOfWork.GetRepository<ITransactionRepository>();
                var roundRepo = unitOfWork.GetRepository<IInterviewRoundRepository>();

                var report = await reportRepo.GetByIdAsync(request.ReportId)
                    ?? throw new NotFoundException("Report not found");

                var room = await roomRepo.GetByIdWithDetailsAsync(report.InterviewRoomId)
                    ?? throw new NotFoundException("Interview room not found");

                var round = await roundRepo.GetByInterviewRoomIdAsync(report.InterviewRoomId);

                report.Status = request.Status;
                report.AdminNote = request.AdminNote;
                report.ResolvedAt = DateTime.UtcNow;
                report.UpdatedAt = DateTime.UtcNow;
                reportRepo.UpdateAsync(report);

                string notificationDetail = "";
                string refundInfo = "No refund was issued for this report.";

                if (request.Status == InterviewReportStatus.Resolved)
                {
                    if (request.RefundOption != null && request.RefundOption != RefundOption.None)
                    {
                        var payment = await transactionRepo.GetByAvailabilityId(room.CurrentAvailabilityId ?? Guid.Empty, TransactionType.Payment);
                        if (payment != null)
                        {
                            int refundAmount = (int)(payment.Amount * (int)request.RefundOption / 100.0);

                            await transactionRepo.AddAsync(new InterviewBookingTransaction
                            {
                                Id = Guid.NewGuid(),
                                OrderCode = RandomGenerator.GenerateOrderCode(),
                                UserId = room.CandidateId ?? report.ReporterId ?? report.ReportedBy,
                                Amount = refundAmount,
                                Type = TransactionType.Refund,
                                Status = TransactionStatus.Paid
                            });

                            notificationDetail = $"Your report has been reviewed and resolved. We have issued a {request.RefundOption}% refund ({refundAmount:N0} resources) to your account. Thank you for your feedback.";
                            refundInfo = $"A {request.RefundOption}% refund ({refundAmount:N0} resources) has been issued to your account.";
                        }
                    }
                    else
                    {
                         notificationDetail = "Your report has been resolved. However, this case does not qualify for a refund based on our review.";
                         refundInfo = "This report was resolved without refund.";
                    }

                    // Cancel the coach payout when admin sides with the candidate.
                    // - Status == Paid: coach was credited (race), reverse the credit and write a negative Earnings audit row.
                    // - Status == PendingPayout: coach was never credited (gated/frozen), just flip to Cancel.
                    var payout = round != null
                        ? await transactionRepo.GetByInterviewRoundId(round.Id, TransactionType.Payout)
                        : null;
                    if (payout != null)
                    {
                        if (payout.Status == TransactionStatus.Paid && room.CoachId.HasValue)
                        {
                            var coachProfileRepo = unitOfWork.GetRepository<ICoachProfileRepository>();
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
                                    InterviewRoundId = round!.Id,
                                    Amount = -payout.Amount,
                                    GrossAmount = payout.GrossAmount.HasValue ? -payout.GrossAmount.Value : null,
                                    CommissionAmount = payout.CommissionAmount.HasValue ? -payout.CommissionAmount.Value : null,
                                    CommissionRate = payout.CommissionRate,
                                    Type = TransactionType.Earnings,
                                    Status = TransactionStatus.Paid,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }

                        payout.Status = TransactionStatus.Cancel;
                        transactionRepo.UpdateAsync(payout);
                    }
                }
                else if (request.Status == InterviewReportStatus.Rejected)
                {
                    // Release the frozen payout: flip PendingPayout -> Paid and credit the coach balance.
                    var payout = round != null
                        ? await transactionRepo.GetByInterviewRoundId(round.Id, TransactionType.Payout)
                        : null;
                    if (payout != null && payout.Status == TransactionStatus.PendingPayout && room.CoachId.HasValue)
                    {
                        var coachProfileRepo = unitOfWork.GetRepository<ICoachProfileRepository>();
                        var coach = await coachProfileRepo.GetProfileByIdAsync(room.CoachId.Value);
                        if (coach != null)
                        {
                            coach.CurrentAmount = (coach.CurrentAmount ?? 0) + payout.Amount;
                            coach.Version++;
                            await coachProfileRepo.UpdateCoachProfileAsync(coach);

                            await transactionRepo.AddAsync(new InterviewBookingTransaction
                            {
                                Id = Guid.NewGuid(),
                                OrderCode = RandomGenerator.GenerateOrderCode(),
                                UserId = room.CoachId.Value,
                                BookingRequestId = payout.BookingRequestId,
                                InterviewRoundId = round!.Id,
                                Amount = payout.Amount,
                                GrossAmount = payout.GrossAmount,
                                CommissionAmount = payout.CommissionAmount,
                                CommissionRate = payout.CommissionRate,
                                Type = TransactionType.Earnings,
                                Status = TransactionStatus.Paid,
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        payout.Status = TransactionStatus.Paid;
                        transactionRepo.UpdateAsync(payout);

                        var coachId = room.CoachId.Value;
                        var roomShortId = room.Id.ToString().Substring(0, 8).ToUpperInvariant();
                        var releasedMessage = $"Your payout for interview room {roomShortId} has been released after report review.";
                        jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                            coachId,
                            NotificationType.PaymentSuccess,
                            "Payout Released",
                            releasedMessage,
                            "/payment-history",
                            null
                        ));
                    }
                   notificationDetail = "Your report has been rejected due to insufficient or unclear information. Please review the details and submit again if necessary.";
                }

                // Send Notification to Reporter
                var reporterId = report.ReporterId ?? report.ReportedBy;
                jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                    reporterId,
                    NotificationType.SystemAnnouncement,
                    "Room Report Result",
                    $"Room {room.Id.ToString().Substring(0, 8)}: {notificationDetail}",
                    null,
                    report.Id
                ));

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitTransactionAsync();

                var reporter = await userRepository.GetByIdAsync(reporterId);
                if (reporter != null)
                {
                    try
                    {
                        var placeholders = new Dictionary<string, string>
                        {
                            ["RecipientName"] = reporter.FullName,
                            ["RoomId"] = room.Id.ToString()[..8].ToUpperInvariant(),
                            ["Status"] = request.Status.ToString(),
                            ["AdminNote"] = request.AdminNote ?? "No additional notes.",
                            ["RefundInfo"] = refundInfo
                        };

                        jobService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                            reporter.Email,
                            "ReportResolution",
                            placeholders));
                    }
                    catch
                    {
                        // Do not fail report resolution flow if email enqueue fails.
                    }
                }
            }
            catch (Exception)
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}

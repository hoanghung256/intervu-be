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

                var report = await reportRepo.GetByIdAsync(request.ReportId)
                    ?? throw new NotFoundException("Report not found");

                var room = await roomRepo.GetByIdWithDetailsAsync(report.InterviewRoomId)
                    ?? throw new NotFoundException("Interview room not found");

                // Update Report Status
                report.Status = request.Status;
                report.AdminNote = request.AdminNote;
                report.ResolvedAt = DateTime.UtcNow;
                report.UpdatedAt = DateTime.UtcNow;
                reportRepo.UpdateAsync(report);

                string notificationDetail = "";
                string refundInfo = "No refund was issued for this report.";

                // Handle Resolve Logic (Refund etc.)
                if (request.Status == InterviewReportStatus.Resolved)
                {
                    if (request.RefundOption != null && request.RefundOption != RefundOption.None)
                    {
                        var payment = await transactionRepo.GetByAvailabilityId(room.CurrentAvailabilityId ?? Guid.Empty, TransactionType.Payment);
                        if (payment != null)
                        {
                            int refundAmount = (int)(payment.Amount * (int)request.RefundOption / 100.0);
                            
                            // Create Refund Transaction
                            await transactionRepo.AddAsync(new InterviewBookingTransaction
                            {
                                Id = Guid.NewGuid(),
                                OrderCode = RandomGenerator.GenerateOrderCode(),
                                UserId = room.CandidateId ?? report.ReporterId ?? report.ReportedBy,
                                CoachAvailabilityId = room.CurrentAvailabilityId,
                                Amount = refundAmount,
                                Type = TransactionType.Refund,
                                Status = TransactionStatus.Paid // Mark as paid immediately for internal resource refund
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

                    // Always Cancel the Payout if Resolved (meaning the coach might be at fault or session was bad)
                    var payout = await transactionRepo.GetByAvailabilityId(room.CurrentAvailabilityId ?? Guid.Empty, TransactionType.Payout);
                    if (payout != null)
                    {
                        payout.Status = TransactionStatus.Cancel;
                        unitOfWork.GetRepository<ITransactionRepository>().UpdateAsync(payout);
                    }
                }
                else if (request.Status == InterviewReportStatus.Rejected)
                {
                    // If Rejected, unfreeze the Payout
                    var payout = await transactionRepo.GetByAvailabilityId(room.CurrentAvailabilityId ?? Guid.Empty, TransactionType.Payout);
                    if (payout != null && payout.Status == TransactionStatus.PendingPayout)
                    {
                        payout.Status = TransactionStatus.Paid; // Or whatever valid state to allow payout
                        unitOfWork.GetRepository<ITransactionRepository>().UpdateAsync(payout);
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
                    "/history",
                    null
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

using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class GetInterviewReportDetail(IUnitOfWork unitOfWork) : IGetInterviewReportDetail
    {
        public async Task<InterviewRoomReportDetailDto> ExecuteAsync(Guid interviewRoomId)
        {
            var reportRepo = unitOfWork.GetRepository<IInterviewReportRepository>();
            var roomRepo = unitOfWork.GetRepository<IInterviewRoomRepository>();
            var transactionRepo = unitOfWork.GetRepository<ITransactionRepository>();
            var userRepo = unitOfWork.GetRepository<IUserRepository>();
            var candidateProfileRepo = unitOfWork.GetRepository<ICandidateProfileRepository>();

            var report = await reportRepo.GetByRoomIdAsync(interviewRoomId)
                ?? throw new NotFoundException("Report not found for this room");

            var room = await roomRepo.GetByIdWithDetailsAsync(interviewRoomId)
                ?? throw new NotFoundException("Interview room not found");

            var candidate = room.CandidateId.HasValue ? await userRepo.GetByIdAsync(room.CandidateId.Value) : null;
            var coach = room.CoachId.HasValue ? await userRepo.GetByIdAsync(room.CoachId.Value) : null;
            var candidateProfile = room.CandidateId.HasValue
                ? await candidateProfileRepo.GetProfileByIdAsync(room.CandidateId.Value)
                : null;

            var paymentTx = room.BookingRequestId.HasValue
                ? await transactionRepo.GetByBookingRequestId(room.BookingRequestId.Value, TransactionType.Payment)
                : null;

            var payoutTx = room.BookingRequestId.HasValue
                ? await transactionRepo.GetByBookingRequestId(room.BookingRequestId.Value, TransactionType.Payout)
                : null;

            var payoutState = payoutTx?.Status ?? room.Transaction?.Status;
            var payoutLocked = payoutState == TransactionStatus.PendingPayout || payoutState == TransactionStatus.Paid;

            return new InterviewRoomReportDetailDto
            {
                ReportId = report.Id,
                InterviewRoomId = report.InterviewRoomId,
                ReporterId = report.ReporterId ?? report.ReportedBy,
                ReporterName = report.Reporter?.FullName ?? string.Empty,
                Reason = report.Reason,
                Details = report.Details,
                ExpectTo = report.ExpectTo,
                Status = report.Status,
                CreatedAt = report.CreatedAt,
                BookingContext = new RoomReportBookingContextDto
                {
                    CoachName = coach?.FullName,
                    CandidateName = candidate?.FullName,
                    ServiceName = room.CoachInterviewService?.InterviewType?.Name,
                    OriginalTime = room.ScheduledTime,
                    CandidateBankBinNumber = candidateProfile?.BankBinNumber,
                    CandidateBankAccountNumber = candidateProfile?.BankAccountNumberMasked,
                },
                FinancialStatus = new RoomReportFinancialStatusDto
                {
                    PaymentStatus = paymentTx?.Status.ToString(),
                    PayOsOrderCode = paymentTx?.OrderCode.ToString(),
                    PayoutLocked = payoutLocked,
                }
            };
        }
    }
}

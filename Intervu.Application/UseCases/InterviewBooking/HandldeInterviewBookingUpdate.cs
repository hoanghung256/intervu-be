using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewBooking
{
    public class HandldeInterviewBookingUpdate : IHandldeInterviewBookingUpdate
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentService _paymentService;
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IBackgroundService _backgroundService;
        private readonly IUnitOfWork _unitOfWork;

        public HandldeInterviewBookingUpdate(ITransactionRepository transactionRepository, IPaymentService paymentService, ICoachAvailabilitiesRepository coachAvailabilitiesRepository, IBackgroundService backgroundService, IUnitOfWork unitOfWork) 
        {
            _transactionRepository = transactionRepository;
            _paymentService = paymentService;
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _backgroundService = backgroundService;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(object webhookPayload)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var (isValid, orderCode) = _paymentService.VerifyPayment(webhookPayload);

                if (!isValid) return;

                var transactionRepo = _unitOfWork.GetRepository<ITransactionRepository>();
                var availabilityRepo = _unitOfWork.GetRepository<ICoachAvailabilitiesRepository>();

                InterviewBookingTransaction transaction = await transactionRepo.GetByOrderCode(orderCode) ?? throw new Exception("Booking transaction not found");

                CoachAvailability availability = await availabilityRepo.GetByIdAsync(transaction.CoachAvailabilityId) ?? throw new Exception("Coach availability not found");

                if (!availability.IsUserAbleToBook(transaction.UserId))
                    throw new CoachAvailabilityNotAvailableException("Availability not able to book");

                availability.Status = CoachAvailabilityStatus.Booked;
                transaction.Status = TransactionStatus.Paid;

                // TODO: Create Interview Room include avai Id (for reschedule purpose)
                _backgroundService.Enqueue<ICreateInterviewRoom>(
                    uc => uc.ExecuteAsync(transaction.UserId, availability.CoachId, availability.StartTime)
                );

                // TODO: Notify candidate and coach

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}

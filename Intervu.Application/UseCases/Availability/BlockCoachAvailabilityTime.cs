using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Availability
{
    public class BlockCoachAvailabilityTime : IBlockCoachAvailabilityTime
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IBookingRequestRepository _bookingRequestRepo;

        public BlockCoachAvailabilityTime(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _availabilityRepo = _unitOfWork.GetRepository<ICoachAvailabilitiesRepository>();
            _transactionRepo = _unitOfWork.GetRepository<ITransactionRepository>();
            _bookingRequestRepo = _unitOfWork.GetRepository<IBookingRequestRepository>();
        }

        public async Task ExecuteAsync(Guid availabilityId, DateTime startTime, DateTime endTime, string? reason)
        {
            if (availabilityId == Guid.Empty)
                throw new BadRequestException("Availability ID must be provided.");

            if (endTime <= startTime)
                throw new BadRequestException("End time must be greater than start time.");

            var availability = await _availabilityRepo.GetByIdAsync(availabilityId)
                ?? throw new NotFoundException($"Coach availability '{availabilityId}' was not found.");

            if (availability.Status != CoachAvailabilityStatus.Available)
                throw new BadRequestException("Only available slots can be partially blocked.");

            if (startTime < availability.StartTime || endTime > availability.EndTime)
                throw new BadRequestException("Blocked range must be inside the original availability slot.");

            availability.BlockedTimes ??= [];

            if (availability.BlockedTimes.Any(bt => bt.StartTime < endTime && bt.EndTime > startTime))
                throw new BadRequestException("Blocked range overlaps an existing blocked interval.");

            var activeTransactions = await _transactionRepo
                .GetActiveBookingsByCoachAsync(availability.CoachId, startTime, endTime);

            var overlapsFlowABooking = activeTransactions.Any(t =>
                t.BookedStartTime.HasValue
                && t.BookedDurationMinutes.HasValue
                && t.BookedStartTime.Value < endTime
                && t.BookedStartTime.Value.AddMinutes(t.BookedDurationMinutes.Value) > startTime);

            if (overlapsFlowABooking)
                throw new BadRequestException("Cannot block this time range because it overlaps an active booking.");

            var activeRounds = await _bookingRequestRepo
                .GetActiveRoundsByCoachAsync(availability.CoachId, startTime, endTime);

            if (activeRounds.Any())
                throw new BadRequestException("Cannot block this time range because it overlaps an active booking request.");

            availability.BlockedTimes.Add(new BlockedTime
            {
                StartTime = startTime,
                EndTime = endTime,
                Reason = reason
            });

            await _unitOfWork.SaveChangesAsync();
        }
    }
}

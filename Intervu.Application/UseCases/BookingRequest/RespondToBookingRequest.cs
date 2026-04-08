using AutoMapper;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class RespondToBookingRequest : IRespondToBookingRequest
    {
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly IMapper _mapper;
        private readonly IBackgroundService _backgroundService;
        private readonly IUserRepository _userRepository;

        public RespondToBookingRequest(
            IBookingRequestRepository bookingRepo,
            IMapper mapper,
            IBackgroundService backgroundService,
            IUserRepository userRepository)
        {
            _bookingRepo = bookingRepo;
            _mapper = mapper;
            _backgroundService = backgroundService;
            _userRepository = userRepository;
        }

        public async Task<BookingRequestDto> ExecuteAsync(Guid coachId, Guid bookingRequestId, RespondToBookingRequestDto dto)
        {
            var bookingRequest = await _bookingRepo.GetByIdWithDetailsAsync(bookingRequestId)
                ?? throw new NotFoundException("Booking request not found");

            // Only the target coach can respond
            if (bookingRequest.CoachId != coachId)
                throw new ForbiddenException("You can only respond to booking requests addressed to you");

            // Only pending requests can be responded to
            if (bookingRequest.Status != BookingRequestStatus.Pending)
                throw new BadRequestException($"Cannot respond to a booking request with status '{bookingRequest.Status}'");

            // Check if the request has expired
            if (bookingRequest.ExpiresAt.HasValue && bookingRequest.ExpiresAt.Value <= DateTime.UtcNow)
            {
                bookingRequest.Status = BookingRequestStatus.Expired;
                bookingRequest.UpdatedAt = DateTime.UtcNow;
                _bookingRepo.UpdateAsync(bookingRequest);
                await _bookingRepo.SaveChangesAsync();
                throw new BadRequestException("This booking request has expired");
            }

            if (dto.IsApproved)
            {
                bookingRequest.Status = BookingRequestStatus.Accepted;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                    throw new BadRequestException("Rejection reason is required when rejecting a booking request");

                bookingRequest.Status = BookingRequestStatus.Rejected;
                bookingRequest.RejectionReason = dto.RejectionReason;
            }

            bookingRequest.RespondedAt = DateTime.UtcNow;
            bookingRequest.UpdatedAt = DateTime.UtcNow;

            _bookingRepo.UpdateAsync(bookingRequest);
            await _bookingRepo.SaveChangesAsync();

            if (!dto.IsApproved)
            {
                try
                {
                    var candidate = await _userRepository.GetByIdAsync(bookingRequest.CandidateId);
                    var coach = await _userRepository.GetByIdAsync(bookingRequest.CoachId);

                    if (candidate != null)
                    {
                        var placeholders = new Dictionary<string, string>
                        {
                            ["CandidateName"] = candidate.FullName,
                            ["CoachName"] = coach?.FullName ?? "Coach",
                            ["RejectionReason"] = bookingRequest.RejectionReason ?? "The coach declined this request."
                        };

                        _backgroundService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                            candidate.Email,
                            "BookingRequestRejected",
                            placeholders));
                    }
                }
                catch
                {
                    // Do not fail booking request response if email enqueue fails.
                }
            }

            var result = _mapper.Map<BookingRequestDto>(bookingRequest);
            result.CandidateName = bookingRequest.Candidate?.User?.FullName;
            result.CoachName = bookingRequest.Coach?.User?.FullName;

            return result;
        }
    }
}

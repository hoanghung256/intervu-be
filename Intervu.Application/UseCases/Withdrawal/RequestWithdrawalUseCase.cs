using Intervu.Application.DTOs.Withdrawal;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Interfaces.UseCases.Withdrawal;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Withdrawal
{
    public class RequestWithdrawalUseCase : IRequestWithdrawal
    {
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundService _jobService;
        private readonly IUnitOfWork _unitOfWork;

        public RequestWithdrawalUseCase(
            ICoachProfileRepository coachProfileRepository,
            IWithdrawalRequestRepository withdrawalRequestRepository,
            ITransactionRepository transactionRepository,
            IPaymentService paymentService,
            IBackgroundService jobService,
            IUnitOfWork unitOfWork)
        {
            _coachProfileRepository = coachProfileRepository;
            _withdrawalRequestRepository = withdrawalRequestRepository;
            _transactionRepository = transactionRepository;
            _paymentService = paymentService;
            _jobService = jobService;
            _unitOfWork = unitOfWork;
        }

        public async Task<WithdrawalResponseDto> ExecuteAsync(Guid userId, RequestWithdrawalDto request)
        {
            if (request.Amount <= 2000)
                throw new ArgumentException("Withdrawal amount must be greater than 2000.");

            var withdrawalRequest = new WithdrawalRequest { Id = Guid.NewGuid() };
            InterviewBookingTransaction? withdrawalTransaction = null;

            // Step 1: Deduct balance and create withdrawal request atomically
            const int maxRetries = 3;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var coach = await _coachProfileRepository.GetProfileByIdAsync(userId)
                        ?? throw new Exception("Coach profile not found.");

                    if (string.IsNullOrWhiteSpace(coach.BankBinNumber) ||
                        string.IsNullOrWhiteSpace(coach.BankAccountNumber))
                    {
                        throw new BadRequestException("Please update your bank information before requesting a withdrawal.");
                    }

                    var currentBalance = coach.CurrentAmount ?? 0;
                    if (currentBalance < request.Amount)
                        throw new InvalidOperationException("Insufficient balance.");

                    // Deduct balance immediately to prevent over-withdrawal
                    coach.CurrentAmount = currentBalance - request.Amount;
                    coach.Version++;
                    await _coachProfileRepository.UpdateCoachProfileAsync(coach);

                    // Create withdrawal request
                    withdrawalRequest.UserId = userId;
                    withdrawalRequest.Amount = request.Amount;
                    withdrawalRequest.Status = WithdrawalStatus.Pending;
                    withdrawalRequest.BankBinNumber = coach.BankBinNumber;
                    withdrawalRequest.BankAccountNumber = coach.BankAccountNumber;
                    withdrawalRequest.Notes = request.Notes;
                    withdrawalRequest.CreatedAt = DateTime.UtcNow;
                    await _withdrawalRequestRepository.AddAsync(withdrawalRequest);

                    // Create withdrawal transaction record
                    withdrawalTransaction = new InterviewBookingTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Amount = request.Amount,
                        Type = TransactionType.Withdrawal,
                        Status = TransactionStatus.PendingWithdrawal,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _transactionRepository.AddAsync(withdrawalTransaction);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    break; // success
                }
                catch (Exception ex) when (attempt < maxRetries - 1 && _unitOfWork.IsConcurrencyException(ex))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    // Retry with fresh entity state
                }
            }

            // Step 2: Call external payment service to transfer money
            try
            {
                await _paymentService.CreateSpendOrderAsync(
                    request.Amount,
                    "WITHDRAWAL",
                    withdrawalRequest.BankBinNumber,
                    withdrawalRequest.BankAccountNumber
                );

                // Payout succeeded
                withdrawalRequest.Status = WithdrawalStatus.Completed;
                withdrawalRequest.ProcessedAt = DateTime.UtcNow;
                _withdrawalRequestRepository.UpdateAsync(withdrawalRequest);

                withdrawalTransaction!.Status = TransactionStatus.Paid;
                _transactionRepository.UpdateAsync(withdrawalTransaction);

                await _transactionRepository.SaveChangesAsync();

                _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                    userId,
                    NotificationType.PaymentSuccess,
                    "Withdrawal Completed",
                    $"Your withdrawal of {request.Amount:N0} resources has been transferred to your bank account.",
                    "/dashboard/wallet",
                    null
                ));
            }
            catch (Exception)
            {
                // Payout failed — restore balance
                await RestoreBalanceAsync(userId, request.Amount, withdrawalRequest, withdrawalTransaction!);

                _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                    userId,
                    NotificationType.PaymentSuccess,
                    "Withdrawal Failed",
                    $"Your withdrawal of {request.Amount:N0} resources failed. The amount has been restored to your balance.",
                    "/dashboard/wallet",
                    null
                ));

                throw;
            }

            return new WithdrawalResponseDto
            {
                Id = withdrawalRequest.Id,
                Amount = withdrawalRequest.Amount,
                Status = withdrawalRequest.Status,
                BankBinNumber = withdrawalRequest.BankBinNumber,
                BankAccountNumber = withdrawalRequest.BankAccountNumber,
                Notes = withdrawalRequest.Notes,
                CreatedAt = withdrawalRequest.CreatedAt,
                ProcessedAt = withdrawalRequest.ProcessedAt
            };
        }

        private async Task RestoreBalanceAsync(
            Guid userId, int amount,
            WithdrawalRequest withdrawalRequest,
            InterviewBookingTransaction withdrawalTransaction)
        {
            const int maxRetries = 3;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var coach = await _coachProfileRepository.GetProfileByIdAsync(userId)
                        ?? throw new Exception("Coach profile not found.");

                    coach.CurrentAmount = (coach.CurrentAmount ?? 0) + amount;
                    coach.Version++;
                    await _coachProfileRepository.UpdateCoachProfileAsync(coach);

                    withdrawalRequest.Status = WithdrawalStatus.Rejected;
                    _withdrawalRequestRepository.UpdateAsync(withdrawalRequest);

                    withdrawalTransaction.Status = TransactionStatus.Cancel;
                    _transactionRepository.UpdateAsync(withdrawalTransaction);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    break;
                }
                catch (Exception ex) when (attempt < maxRetries - 1 && _unitOfWork.IsConcurrencyException(ex))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
            }
        }
    }
}

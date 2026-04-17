using Intervu.Application.DTOs.Withdrawal;
using Intervu.Application.Interfaces.UseCases.Withdrawal;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Withdrawal
{
    public class GetWithdrawalHistoryUseCase : IGetWithdrawalHistory
    {
        private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;

        public GetWithdrawalHistoryUseCase(IWithdrawalRequestRepository withdrawalRequestRepository)
        {
            _withdrawalRequestRepository = withdrawalRequestRepository;
        }

        public async Task<(IReadOnlyList<WithdrawalResponseDto> Items, int TotalCount)> ExecuteAsync(
            Guid userId, int page, int pageSize)
        {
            var (items, totalCount) = await _withdrawalRequestRepository.GetPagedByUserAsync(userId, page, pageSize);

            var dtos = items.Select(x => new WithdrawalResponseDto
            {
                Id = x.Id,
                Amount = x.Amount,
                Status = x.Status,
                BankBinNumber = x.BankBinNumber,
                BankAccountNumber = x.BankAccountNumberMasked,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                ProcessedAt = x.ProcessedAt
            }).ToList();

            return (dtos, totalCount);
        }
    }
}

using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.Common;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetAllTransactionsForAdmin : IGetAllTransactionsForAdmin
    {
        private readonly ITransactionRepository _transactionRepository;

        public GetAllTransactionsForAdmin(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<PagedResult<AdminTransactionDto>> ExecuteAsync(
            int page,
            int pageSize,
            TransactionType? type = null,
            TransactionStatus? status = null)
        {
            var (items, total) = await _transactionRepository.GetPagedForAdminAsync(page, pageSize, type, status);

            var dtos = new List<AdminTransactionDto>();

            foreach (var t in items)
            {
                var dto = new AdminTransactionDto
                {
                    Id = t.Id,
                    OrderCode = t.OrderCode,
                    Type = t.Type.ToString(),
                    Status = t.Status.ToString(),
                    Amount = t.Amount,
                    UserId = t.UserId,
                    UserName = t.User?.FullName ?? string.Empty,
                    UserEmail = t.User?.Email ?? string.Empty,
                    CreatedAt = t.CreatedAt,
                };

                if (t.BookingRequest is Domain.Entities.BookingRequest br)
                {
                    dto.BookingRequestId = br.Id;
                    dto.BookingTotalAmount = br.TotalAmount;
                    dto.CandidateName = br.Candidate?.User?.FullName;
                    dto.CoachName = br.Coach?.User?.FullName;
                }

                dtos.Add(dto);
            }

            return new PagedResult<AdminTransactionDto>(dtos, total, pageSize, page);
        }
    }
}

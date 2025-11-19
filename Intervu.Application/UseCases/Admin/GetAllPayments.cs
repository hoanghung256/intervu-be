using AutoMapper;
using Intervu.Application.Common;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetAllPayments : IGetAllPayments
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        public GetAllPayments(IPaymentRepository paymentRepository, IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<PaymentDto>> ExecuteAsync(int page, int pageSize)
        {
            var pagedPayments = await _paymentRepository.GetPagedPaymentsAsync(page, pageSize);

            var paymentDtos = _mapper.Map<List<PaymentDto>>(pagedPayments.Items);

            return new PagedResult<PaymentDto>(paymentDtos, pagedPayments.TotalItems, pageSize, page);
        }
    }
}

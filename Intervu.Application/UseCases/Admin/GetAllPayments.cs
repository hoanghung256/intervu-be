using AutoMapper;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Application.DTOs.Common;

namespace Intervu.Application.UseCases.Admin
{
    public class GetAllPayments : IGetAllPayments
    {
        //private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        //public GetAllPayments(IPaymentRepository paymentRepository, IMapper mapper)
        public GetAllPayments(IMapper mapper)
        {
            //_paymentRepository = paymentRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<PaymentDto>> ExecuteAsync(int page, int pageSize)
        {
            //var (items, total) = await _paymentRepository.GetPagedPaymentsAsync(page, pageSize);

            //var paymentDtos = _mapper.Map<List<PaymentDto>>(items);
            await Task.CompletedTask;

            return new PagedResult<PaymentDto>(new List<PaymentDto>(), 0, pageSize, page);
        }
    }
}

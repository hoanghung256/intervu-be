using AutoMapper;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Admin;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetDashboardStats : IGetDashboardStats
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IInterviewerProfileRepository _interviewerProfileRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IFeedbackRepository _feedbackRepository;

        public GetDashboardStats(
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IInterviewerProfileRepository interviewerProfileRepository,
            IPaymentRepository paymentRepository,
            IFeedbackRepository feedbackRepository)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _interviewerProfileRepository = interviewerProfileRepository;
            _paymentRepository = paymentRepository;
            _feedbackRepository = feedbackRepository;
        }

        public async Task<DashboardStatsDto> ExecuteAsync()
        {
            var totalUsers = await _userRepository.GetTotalUsersCountAsync();
            var totalCompanies = await _companyRepository.GetTotalCompaniesCountAsync();
            var totalInterviewers = await _interviewerProfileRepository.GetTotalInterviewersCountAsync();
            var totalPayments = await _paymentRepository.GetTotalPaymentsCountAsync();
            var totalRevenue = await _paymentRepository.GetTotalRevenueAsync();
            var totalFeedbacks = await _feedbackRepository.GetTotalFeedbacksCountAsync();
            var averageRating = await _feedbackRepository.GetAverageRatingAsync();

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalCompanies = totalCompanies,
                TotalInterviewers = totalInterviewers,
                TotalPayments = totalPayments,
                TotalRevenue = totalRevenue,
                TotalFeedbacks = totalFeedbacks,
                AverageRating = averageRating
            };
        }
    }
}

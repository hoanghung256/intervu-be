using System.Security.Claims;
using Intervu.API.Controllers.v1.Payment;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Intervu.API.Test
{
    public class InterviewBookingControllerTests
    {
        [Fact]
        public async Task GetHistory_ReturnsPagedResult_ForAuthenticatedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pagedResult = new PagedResult<InterviewBookingTransactionHistoryDto>(
                new List<InterviewBookingTransactionHistoryDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        OrderCode = 123,
                        UserId = userId,
                        Amount = 100,
                        Type = TransactionType.Payment,
                        Status = TransactionStatus.Paid
                    }
                },
                totalItems: 1,
                pageSize: 10,
                currentPage: 1);

            var historyUseCase = new FakeGetInterviewBookingHistory(pagedResult);

            var controller = new InterviewBookingController(
                new NullLogger<InterviewBookingController>(),
                new DummyCreateBookingCheckoutUrl(),
                new DummyGetInterviewBooking(),
                new DummyHandleInterviewBookingUpdate(),
                new DummyCancelInterview(),
                historyUseCase);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim(ClaimTypes.Role, UserRole.Candidate.ToString())
                    }, "TestAuth"))
                }
            };

            var request = new GetInterviewBookingHistoryRequest { Page = 1, PageSize = 10 };

            // Act
            var result = await controller.GetHistory(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = ok.Value!;

            var successProp = payload.GetType().GetProperty("success")?.GetValue(payload);
            Assert.Equal(true, successProp);

            var dataProp = payload.GetType().GetProperty("data")?.GetValue(payload);
            var data = Assert.IsType<PagedResult<InterviewBookingTransactionHistoryDto>>(dataProp);
            Assert.Single(data.Items);
            Assert.Equal(pagedResult.TotalItems, data.TotalItems);
            Assert.Equal(pagedResult.Items.First().OrderCode, data.Items.First().OrderCode);
        }

        private class FakeGetInterviewBookingHistory : IGetInterviewBookingHistory
        {
            private readonly PagedResult<InterviewBookingTransactionHistoryDto> _result;

            public FakeGetInterviewBookingHistory(PagedResult<InterviewBookingTransactionHistoryDto> result)
            {
                _result = result;
            }

            public Task<PagedResult<InterviewBookingTransactionHistoryDto>> ExecuteAsync(Guid userId, GetInterviewBookingHistoryRequest request)
            {
                return Task.FromResult(_result);
            }
        }

        private class DummyCreateBookingCheckoutUrl : ICreateBookingCheckoutUrl
        {
            public Task<string?> ExecuteAsync(Guid candidateId,
                Guid coachId,
                Guid coachAvailabilityId,
                Guid coachInterviewServiceId,
                DateTime startTime,
                string returnUrl)
            {
                return Task.FromResult<string?>(null);
            }
        }

        private class DummyGetInterviewBooking : IGetInterviewBooking
        {
            public Task<InterviewBookingTransaction?> Get(int orderCode, TransactionType type)
            {
                return Task.FromResult<InterviewBookingTransaction?>(null);
            }

            public Task<InterviewBookingTransaction?> GetById(Guid id)
            {
                return Task.FromResult<InterviewBookingTransaction?>(null);
            }
        }

        private class DummyHandleInterviewBookingUpdate : IHandldeInterviewBookingUpdate
        {
            public Task ExecuteAsync(object payload)
            {
                return Task.CompletedTask;
            }
        }

        private class DummyCancelInterview : ICancelInterview
        {
            public Task<int> ExecuteAsync(Guid interviewRoomId)
            {
                return Task.FromResult(0);
            }
        }
    }
}

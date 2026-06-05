using Intervu.Application.Exceptions;
using Intervu.Application.Utils;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Xunit;

namespace Intervu.API.Test.UnitTests.Application.Utils
{
    public class InterviewTypeBookabilityTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void EnsureActiveForCoachServices_Throws_WhenNotActive()
        {
            var type = new InterviewType { Name = "X", Status = InterviewTypeStatus.Inactive };
            var ex = Assert.Throws<BadRequestException>(() => InterviewTypeBookability.EnsureActiveForCoachServices(type));
            Assert.Contains("not active", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void EnsureActiveForBooking_Throws_WhenDeprecated()
        {
            var type = new InterviewType { Name = "Y", Status = InterviewTypeStatus.Deprecated };
            var ex = Assert.Throws<BadRequestException>(() => InterviewTypeBookability.EnsureActiveForBooking(type));
            Assert.Contains("not available for booking", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void EnsureActiveForCoachServices_DoesNotThrow_WhenActive()
        {
            var type = new InterviewType { Name = "Z", Status = InterviewTypeStatus.Active };
            InterviewTypeBookability.EnsureActiveForCoachServices(type);
        }
    }
}

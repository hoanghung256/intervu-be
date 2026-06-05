using AutoMapper;
using Intervu.Application.Exceptions;
using Intervu.Application.Mappings;
using Intervu.Application.UseCases.CoachInterviewService;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Moq;
using Xunit;

namespace Intervu.API.Test.UnitTests.Application.CoachInterviewServiceUseCases
{
    public class GetCoachInterviewServicesTests
    {
        private static IMapper CreateMapper() =>
            new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()));

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_Public_ExcludesNonActiveInterviewTypes()
        {
            var coachId = Guid.NewGuid();
            var serviceRepo = new Mock<ICoachInterviewServiceRepository>();
            var coachRepo = new Mock<ICoachProfileRepository>();
            coachRepo.Setup(x => x.GetProfileByIdAsync(coachId))
                .ReturnsAsync(new CoachProfile { Id = coachId });
            serviceRepo.Setup(x => x.GetByCoachIdAsync(coachId)).ReturnsAsync(new List<CoachInterviewService>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CoachId = coachId,
                    InterviewTypeId = Guid.NewGuid(),
                    Price = 10,
                    DurationMinutes = 60,
                    InterviewType = new InterviewType
                    {
                        Name = "Off",
                        Status = InterviewTypeStatus.Inactive
                    }
                }
            });

            var sut = new GetCoachInterviewServices(serviceRepo.Object, coachRepo.Object, CreateMapper());

            var result = (await sut.ExecuteAsync(coachId, includeUnavailableForCoach: false)).ToList();

            Assert.Empty(result);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_Mine_IncludesNonActiveInterviewTypes()
        {
            var coachId = Guid.NewGuid();
            var typeId = Guid.NewGuid();
            var serviceRepo = new Mock<ICoachInterviewServiceRepository>();
            var coachRepo = new Mock<ICoachProfileRepository>();
            serviceRepo.Setup(x => x.GetByCoachIdAsync(coachId)).ReturnsAsync(new List<CoachInterviewService>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CoachId = coachId,
                    InterviewTypeId = typeId,
                    Price = 10,
                    DurationMinutes = 60,
                    InterviewType = new InterviewType
                    {
                        Name = "Off",
                        Status = InterviewTypeStatus.Deprecated
                    }
                }
            });

            var sut = new GetCoachInterviewServices(serviceRepo.Object, coachRepo.Object, CreateMapper());

            var result = (await sut.ExecuteAsync(coachId, includeUnavailableForCoach: true)).ToList();

            Assert.Single(result);
            Assert.False(result[0].IsBookable);
            Assert.Equal(InterviewTypeStatus.Deprecated, result[0].InterviewTypeStatus);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_Public_Throws_WhenCoachMissing()
        {
            var coachId = Guid.NewGuid();
            var serviceRepo = new Mock<ICoachInterviewServiceRepository>();
            var coachRepo = new Mock<ICoachProfileRepository>();
            coachRepo.Setup(x => x.GetProfileByIdAsync(coachId)).ReturnsAsync((CoachProfile?)null);

            var sut = new GetCoachInterviewServices(serviceRepo.Object, coachRepo.Object, CreateMapper());

            await Assert.ThrowsAsync<NotFoundException>(() => sut.ExecuteAsync(coachId, false));
        }
    }
}

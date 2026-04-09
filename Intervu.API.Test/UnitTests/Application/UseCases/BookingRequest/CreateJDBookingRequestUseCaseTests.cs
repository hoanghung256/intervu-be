using AutoMapper;
using Intervu.Application;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Application.Mappings;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using DomainBookingRequest = Intervu.Domain.Entities.BookingRequest;

namespace Intervu.API.Test.UnitTests.Application.UseCases.BookingRequest
{
    public class CreateJDBookingRequestUseCaseTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_SingleRound_CreatesBookingAndBooksBlocks()
        {
            var candidateId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var block1Id = Guid.NewGuid();
            var block2Id = Guid.NewGuid();
            var start = DateTime.UtcNow.AddDays(2).Date.AddHours(9);

            var dto = new CreateJDBookingRequestDto
            {
                CoachId = coachId,
                JobDescriptionUrl = "https://example.com/jd.pdf",
                CVUrl = "https://example.com/cv.pdf",
                AimLevel = AimLevel.Senior,
                Rounds =
                [
                    new CreateInterviewRoundDto
                    {
                        CoachInterviewServiceId = serviceId,
                        AvailabilityIds = [block1Id, block2Id]
                    }
                ]
            };

            var service = BuildServiceProviderForCreateBooking(
                coachId,
                new[]
                {
                    new CoachInterviewService
                    {
                        Id = serviceId,
                        CoachId = coachId,
                        Price = 120,
                        DurationMinutes = 60,
                        InterviewType = new InterviewType { Name = "System Design", IsCoding = false }
                    }
                },
                new Dictionary<Guid, CoachAvailability>
                {
                    [block1Id] = new CoachAvailability
                    {
                        Id = block1Id,
                        CoachId = coachId,
                        StartTime = start,
                        EndTime = start.AddMinutes(30),
                        Status = CoachAvailabilityStatus.Available
                    },
                    [block2Id] = new CoachAvailability
                    {
                        Id = block2Id,
                        CoachId = coachId,
                        StartTime = start.AddMinutes(30),
                        EndTime = start.AddMinutes(60),
                        Status = CoachAvailabilityStatus.Available
                    }
                });

            var useCase = service.Provider.GetRequiredService<ICreateJDBookingRequest>();

            var result = await useCase.ExecuteAsync(candidateId, dto);

            Assert.Equal(BookingRequestStatus.Pending, result.Status);
            Assert.Equal(120, result.TotalAmount);
            Assert.NotNull(result.Rounds);
            Assert.Single(result.Rounds!);
            Assert.Equal("Candidate Unit", result.CandidateName);
            Assert.Equal("Coach Unit", result.CoachName);

            service.UnitOfWorkRepo.Verify(x => x.BeginTransactionAsync(), Times.Once);
            service.UnitOfWorkRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            service.UnitOfWorkRepo.Verify(x => x.CommitTransactionAsync(), Times.Once);
            service.UnitOfWorkRepo.Verify(x => x.RollbackTransactionAsync(), Times.Never);
            service.AvailabilityRepo.Verify(x => x.UpdateAsync(It.IsAny<CoachAvailability>()), Times.Exactly(2));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_MultipleRounds_CreatesBookingWithAllRounds()
        {
            var candidateId = Guid.NewGuid();
            var coachId = Guid.NewGuid();
            var service1Id = Guid.NewGuid();
            var service2Id = Guid.NewGuid();
            var block1Id = Guid.NewGuid();
            var block2Id = Guid.NewGuid();
            var block3Id = Guid.NewGuid();
            var start = DateTime.UtcNow.AddDays(2).Date.AddHours(13);

            var dto = new CreateJDBookingRequestDto
            {
                CoachId = coachId,
                JobDescriptionUrl = "https://example.com/jd.pdf",
                CVUrl = "https://example.com/cv.pdf",
                AimLevel = AimLevel.MidLevel,
                Rounds =
                [
                    new CreateInterviewRoundDto
                    {
                        CoachInterviewServiceId = service1Id,
                        AvailabilityIds = [block1Id, block2Id]
                    },
                    new CreateInterviewRoundDto
                    {
                        CoachInterviewServiceId = service2Id,
                        AvailabilityIds = [block3Id]
                    }
                ]
            };

            var service = BuildServiceProviderForCreateBooking(
                coachId,
                new[]
                {
                    new CoachInterviewService
                    {
                        Id = service1Id,
                        CoachId = coachId,
                        Price = 100,
                        DurationMinutes = 60,
                        InterviewType = new InterviewType { Name = "Tech", IsCoding = true }
                    },
                    new CoachInterviewService
                    {
                        Id = service2Id,
                        CoachId = coachId,
                        Price = 80,
                        DurationMinutes = 30,
                        InterviewType = new InterviewType { Name = "Behavioral", IsCoding = false }
                    }
                },
                new Dictionary<Guid, CoachAvailability>
                {
                    [block1Id] = new CoachAvailability
                    {
                        Id = block1Id,
                        CoachId = coachId,
                        StartTime = start,
                        EndTime = start.AddMinutes(30),
                        Status = CoachAvailabilityStatus.Available
                    },
                    [block2Id] = new CoachAvailability
                    {
                        Id = block2Id,
                        CoachId = coachId,
                        StartTime = start.AddMinutes(30),
                        EndTime = start.AddMinutes(60),
                        Status = CoachAvailabilityStatus.Available
                    },
                    [block3Id] = new CoachAvailability
                    {
                        Id = block3Id,
                        CoachId = coachId,
                        StartTime = start.AddHours(1),
                        EndTime = start.AddHours(1).AddMinutes(30),
                        Status = CoachAvailabilityStatus.Available
                    }
                });

            var useCase = service.Provider.GetRequiredService<ICreateJDBookingRequest>();

            var result = await useCase.ExecuteAsync(candidateId, dto);

            Assert.Equal(BookingRequestStatus.Pending, result.Status);
            Assert.Equal(180, result.TotalAmount);
            Assert.NotNull(result.Rounds);
            Assert.Equal(2, result.Rounds!.Count);
            Assert.Equal(1, result.Rounds[0].RoundNumber);
            Assert.Equal(2, result.Rounds[1].RoundNumber);

            service.UnitOfWorkRepo.Verify(x => x.BeginTransactionAsync(), Times.Once);
            service.UnitOfWorkRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            service.UnitOfWorkRepo.Verify(x => x.CommitTransactionAsync(), Times.Once);
            service.AvailabilityRepo.Verify(x => x.UpdateAsync(It.IsAny<CoachAvailability>()), Times.Exactly(3));
        }

        private static BookingCreateContext BuildServiceProviderForCreateBooking(
            Guid coachId,
            IEnumerable<CoachInterviewService> services,
            Dictionary<Guid, CoachAvailability> availabilityById)
        {
            var bookingRepo = new Mock<IBookingRequestRepository>();
            var coachServiceRepo = new Mock<ICoachInterviewServiceRepository>();
            var coachProfileRepo = new Mock<ICoachProfileRepository>();
            var availabilityRepo = new Mock<ICoachAvailabilitiesRepository>();
            var unitOfWorkRepo = new Mock<IUnitOfWork>();

            DomainBookingRequest? addedBooking = null;
            var serviceList = services.ToList();
            var serviceMap = serviceList.ToDictionary(s => s.Id);

            coachProfileRepo.Setup(x => x.GetProfileByIdAsync(coachId))
                .ReturnsAsync(new CoachProfile { Id = coachId });

            coachServiceRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync((IEnumerable<Guid> ids) => serviceList.Where(s => ids.Contains(s.Id)).ToList());

            availabilityRepo.Setup(x => x.GetByIdForUpdateAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => availabilityById.TryGetValue(id, out var block) ? block : null);

            bookingRepo.Setup(x => x.AddAsync(It.IsAny<DomainBookingRequest>()))
                .Callback<DomainBookingRequest>(b => addedBooking = b)
                .Returns(Task.CompletedTask);

            bookingRepo.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) =>
                {
                    if (addedBooking == null || addedBooking.Id != id)
                    {
                        return null;
                    }

                    addedBooking.Candidate = new CandidateProfile
                    {
                        Id = addedBooking.CandidateId,
                        User = new User
                        {
                            Id = addedBooking.CandidateId,
                            FullName = "Candidate Unit",
                            Email = "candidate@test.com",
                            Password = "pwd"
                        }
                    };
                    addedBooking.Coach = new CoachProfile
                    {
                        Id = addedBooking.CoachId,
                        User = new User
                        {
                            Id = addedBooking.CoachId,
                            FullName = "Coach Unit",
                            Email = "coach@test.com",
                            Password = "pwd"
                        }
                    };

                    foreach (var round in addedBooking.Rounds)
                    {
                        round.CoachInterviewService = serviceMap[round.CoachInterviewServiceId];
                    }

                    return addedBooking;
                });

            unitOfWorkRepo.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkRepo.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkRepo.Setup(x => x.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiClients:AIService"] = "https://example.com"
                })
                .Build();

            serviceCollection.AddUseCases(config);
            serviceCollection.AddSingleton<IMapper>(new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())));
            serviceCollection.AddScoped(_ => bookingRepo.Object);
            serviceCollection.AddScoped(_ => coachServiceRepo.Object);
            serviceCollection.AddScoped(_ => coachProfileRepo.Object);
            serviceCollection.AddScoped(_ => availabilityRepo.Object);
            serviceCollection.AddScoped(_ => unitOfWorkRepo.Object);

            var provider = serviceCollection.BuildServiceProvider();

            return new BookingCreateContext(provider, availabilityRepo, unitOfWorkRepo);
        }

        private sealed record BookingCreateContext(
            ServiceProvider Provider,
            Mock<ICoachAvailabilitiesRepository> AvailabilityRepo,
            Mock<IUnitOfWork> UnitOfWorkRepo);
    }
}

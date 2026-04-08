using Intervu.Application.DTOs.Availability;
using Intervu.Application.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Moq;

namespace Intervu.API.Test.UnitTests.Application.UseCases.Availability
{
    public class AvailabilityManagementUseCasesTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public async Task CreateCoachAvailability_ValidRange_CreatesExpectedBlocks()
        {
            var coachId = Guid.NewGuid();
            var startUtc = DateTime.UtcNow.Date.AddDays(2).AddHours(9);
            var start = new DateTimeOffset(startUtc, TimeSpan.Zero);
            var end = start.AddHours(2);
            var dto = new CoachAvailabilityCreateDto
            {
                CoachId = coachId,
                RangeStartTime = start,
                RangeEndTime = end
            };

            var availabilityRepo = new Mock<ICoachAvailabilitiesRepository>();
            var coachRepo = new Mock<ICoachProfileRepository>();
            List<CoachAvailability>? createdBlocks = null;

            coachRepo.Setup(x => x.GetProfileByIdAsync(coachId))
                .ReturnsAsync(new CoachProfile { Id = coachId });
            availabilityRepo.Setup(x => x.IsCoachAvailableAsync(
                    coachId,
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset>(),
                    null))
                .ReturnsAsync(true);
            availabilityRepo.Setup(x => x.CreateMultipleCoachAvailabilitiesAsync(It.IsAny<List<CoachAvailability>>()))
                .Callback<List<CoachAvailability>>(blocks =>
                {
                    createdBlocks = blocks;
                    foreach (var block in blocks)
                    {
                        block.Id = Guid.NewGuid();
                    }
                })
                .ReturnsAsync(Guid.NewGuid());

            var useCase = new CreateCoachAvailability(availabilityRepo.Object, coachRepo.Object);

            var result = await useCase.ExecuteAsync(dto);

            Assert.Equal(4, result.Count);
            Assert.NotNull(createdBlocks);
            Assert.Equal(4, createdBlocks!.Count);
            Assert.All(createdBlocks, b => Assert.Equal(CoachAvailabilityStatus.Available, b.Status));
            availabilityRepo.Verify(x => x.CreateMultipleCoachAvailabilitiesAsync(It.IsAny<List<CoachAvailability>>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task CreateCoachAvailability_OverlappingRange_ThrowsArgumentException()
        {
            var coachId = Guid.NewGuid();
            var startUtc = DateTime.UtcNow.Date.AddDays(2).AddHours(9);
            var start = new DateTimeOffset(startUtc, TimeSpan.Zero);
            var end = start.AddHours(1);
            var dto = new CoachAvailabilityCreateDto
            {
                CoachId = coachId,
                RangeStartTime = start,
                RangeEndTime = end
            };

            var availabilityRepo = new Mock<ICoachAvailabilitiesRepository>();
            var coachRepo = new Mock<ICoachProfileRepository>();

            coachRepo.Setup(x => x.GetProfileByIdAsync(coachId))
                .ReturnsAsync(new CoachProfile { Id = coachId });
            availabilityRepo.Setup(x => x.IsCoachAvailableAsync(
                    coachId,
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset>(),
                    null))
                .ReturnsAsync(false);

            var useCase = new CreateCoachAvailability(availabilityRepo.Object, coachRepo.Object);

            var act = async () => await useCase.ExecuteAsync(dto);

            await Assert.ThrowsAsync<ArgumentException>(act);
            availabilityRepo.Verify(x => x.CreateMultipleCoachAvailabilitiesAsync(It.IsAny<List<CoachAvailability>>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task UpdateCoachAvailability_RemovedBookedSlot_ThrowsArgumentException()
        {
            var coachId = Guid.NewGuid();
            var baseTime = DateTime.UtcNow.Date.AddDays(2).AddHours(10);
            var dto = new CoachAvailabilityUpdateDto
            {
                CoachId = coachId,
                OriginalStartTime = new DateTimeOffset(baseTime, TimeSpan.Zero),
                OriginalEndTime = new DateTimeOffset(baseTime.AddHours(1), TimeSpan.Zero),
                NewStartTime = new DateTimeOffset(baseTime.AddMinutes(30), TimeSpan.Zero),
                NewEndTime = new DateTimeOffset(baseTime.AddHours(1), TimeSpan.Zero)
            };

            var existingBlocks = new List<CoachAvailability>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CoachId = coachId,
                    StartTime = baseTime,
                    EndTime = baseTime.AddMinutes(30),
                    Status = CoachAvailabilityStatus.Booked
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CoachId = coachId,
                    StartTime = baseTime.AddMinutes(30),
                    EndTime = baseTime.AddHours(1),
                    Status = CoachAvailabilityStatus.Available
                }
            };

            var availabilityRepo = new Mock<ICoachAvailabilitiesRepository>();
            availabilityRepo.Setup(x => x.GetBlocksInRangeAsync(coachId, baseTime, baseTime.AddHours(1)))
                .ReturnsAsync(existingBlocks);

            var useCase = new UpdateCoachAvailability(availabilityRepo.Object);

            var act = async () => await useCase.ExecuteAsync(dto);

            await Assert.ThrowsAsync<ArgumentException>(act);
            availabilityRepo.Verify(x => x.DeleteMultipleAsync(It.IsAny<List<Guid>>()), Times.Never);
            availabilityRepo.Verify(x => x.CreateMultipleCoachAvailabilitiesAsync(It.IsAny<List<CoachAvailability>>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task DeleteCoachAvailability_AvailableSlot_ReturnsTrue()
        {
            var availabilityId = Guid.NewGuid();
            var availabilityRepo = new Mock<ICoachAvailabilitiesRepository>();

            availabilityRepo.Setup(x => x.GetByIdAsync(availabilityId))
                .ReturnsAsync(new CoachAvailability
                {
                    Id = availabilityId,
                    Status = CoachAvailabilityStatus.Available
                });
            availabilityRepo.Setup(x => x.DeleteCoachAvailabilityAsync(availabilityId))
                .ReturnsAsync(true);

            var useCase = new DeleteCoachAvailability(availabilityRepo.Object);

            var result = await useCase.ExecuteAsync(availabilityId);

            Assert.True(result);
            availabilityRepo.Verify(x => x.DeleteCoachAvailabilityAsync(availabilityId), Times.Once);
        }
    }
}

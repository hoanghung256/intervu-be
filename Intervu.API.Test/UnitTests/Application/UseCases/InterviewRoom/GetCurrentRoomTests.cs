using AutoMapper;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Mappings;
using Intervu.Application.UseCases.InterviewRoom;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Moq;
using DomainBookingRequest = Intervu.Domain.Entities.BookingRequest;
using DomainCoachInterviewService = Intervu.Domain.Entities.CoachInterviewService;
using DomainInterviewRoom = Intervu.Domain.Entities.InterviewRoom;
using DomainInterviewType = Intervu.Domain.Entities.InterviewType;

namespace Intervu.API.Test.UnitTests.Application.UseCases.InterviewRoomUseCase
{
    public class GetCurrentRoomTests
    {
        private readonly IMapper _mapper;

        public GetCurrentRoomTests()
        {
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_RoomNotFound_ReturnsNull()
        {
            // Arrange
            var roomId = Guid.NewGuid();
            var roomRepo = new Mock<IInterviewRoomRepository>();
            roomRepo.Setup(x => x.GetByIdWithDetailsAsync(roomId)).ReturnsAsync((DomainInterviewRoom?)null);
            var useCase = new GetCurrentRoom(roomRepo.Object, _mapper);

            // Act
            var result = await useCase.ExecuteAsync(roomId);

            // Assert
            Assert.Null(result);
            roomRepo.Verify(x => x.GetByIdWithDetailsAsync(roomId), Times.Once);
            roomRepo.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_RoomWithBookingRequest_MapsJdAndCvLinks()
        {
            // Arrange
            var roomId = Guid.NewGuid();
            var roomRepo = new Mock<IInterviewRoomRepository>();
            var room = new DomainInterviewRoom
            {
                Id = roomId,
                CandidateId = Guid.NewGuid(),
                CoachId = Guid.NewGuid(),
                BookingRequestId = Guid.NewGuid(),
                Status = InterviewRoomStatus.Scheduled,
                BookingRequest = new DomainBookingRequest
                {
                    JobDescriptionUrl = "https://cdn.example.com/jd-001.pdf",
                    CVUrl = "https://cdn.example.com/cv-001.pdf",
                },
                CoachInterviewService = new DomainCoachInterviewService
                {
                    InterviewType = new DomainInterviewType
                    {
                        Name = "Coding",
                    },
                },
            };

            roomRepo.Setup(x => x.GetByIdWithDetailsAsync(roomId)).ReturnsAsync(room);
            var useCase = new GetCurrentRoom(roomRepo.Object, _mapper);

            // Act
            var result = await useCase.ExecuteAsync(roomId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<InterviewRoomDto>(result);
            Assert.Equal(roomId, result!.Id);
            Assert.Equal(room.BookingRequestId, result.BookingRequestId);
            Assert.Equal("https://cdn.example.com/jd-001.pdf", result.JobDescriptionUrl);
            Assert.Equal("https://cdn.example.com/cv-001.pdf", result.CVUrl);
            Assert.Equal("Coding", result.InterviewTypeName);
            roomRepo.Verify(x => x.GetByIdWithDetailsAsync(roomId), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ExecuteAsync_RoomWithoutBookingRequest_ReturnsNullJdCvLinks()
        {
            // Arrange
            var roomId = Guid.NewGuid();
            var roomRepo = new Mock<IInterviewRoomRepository>();
            var room = new DomainInterviewRoom
            {
                Id = roomId,
                Status = InterviewRoomStatus.Scheduled,
                BookingRequestId = null,
                BookingRequest = null,
            };

            roomRepo.Setup(x => x.GetByIdWithDetailsAsync(roomId)).ReturnsAsync(room);
            var useCase = new GetCurrentRoom(roomRepo.Object, _mapper);

            // Act
            var result = await useCase.ExecuteAsync(roomId);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.JobDescriptionUrl);
            Assert.Null(result.CVUrl);
            roomRepo.Verify(x => x.GetByIdWithDetailsAsync(roomId), Times.Once);
        }
    }
}

using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Assessment;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AssessmentController
{
    public class ViewPersonalizedRoadmapTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewPersonalizedRoadmapTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private static SurveyRoadmapDto BuildRoadmapSeed()
        {
            return new SurveyRoadmapDto
            {
                RoadmapMetadata = new SurveyRoadmapMetadataDto
                {
                    TargetRole = "Backend Engineer",
                    TargetLevel = "Middle",
                    TotalPhases = 1
                },
                Phases = new List<SurveyRoadmapPhaseDto>
                {
                    new SurveyRoadmapPhaseDto
                    {
                        PhaseId = "phase-1",
                        PhaseName = "Foundations",
                        Nodes = new List<SurveyRoadmapNodeDto>
                        {
                            new SurveyRoadmapNodeDto
                            {
                                SkillId = "skill-dotnet",
                                SkillName = ".NET",
                                Assessment = new SurveyRoadmapNodeAssessmentDto
                                {
                                    CurrentLevel = "Basic",
                                    TargetLevel = "Intermediate",
                                    SfiaLevel = 2,
                                    Status = "Weak",
                                    Progress = 45
                                }
                            }
                        }
                    }
                }
            };
        }

        private async Task SeedSnapshotWithRoadmapAsync(Guid userId)
        {
            var processResponse = await _api.PostAsync("/api/v1/assessment/process", new SurveyResponsesDto
            {
                UserId = userId,
                Target = new SurveyTargetDto
                {
                    Roles = new List<string> { "Backend Engineer" },
                    Level = "Middle",
                    SkillsTarget = new List<string> { ".NET" }
                },
                Current = new SurveyCurrentDto
                {
                    Skills = new List<SurveySkillLevelDto>
                    {
                        new SurveySkillLevelDto { Skill = ".NET", Level = "Basic", SfiaLevel = 1 }
                    }
                },
                Gap = new SurveyGapDto
                {
                    Missing = new List<string>(),
                    Weak = new List<string> { ".NET" }
                },
                Roadmap = BuildRoadmapSeed()
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, processResponse.StatusCode, "Roadmap snapshot seed returns 200 OK");
        }

        private async Task SeedSnapshotWithoutRoadmapAsync(Guid userId)
        {
            var processResponse = await _api.PostAsync("/api/v1/assessment/process", new SurveyResponsesDto
            {
                UserId = userId,
                Target = new SurveyTargetDto
                {
                    Roles = new List<string> { "Backend Engineer" },
                    Level = "Junior",
                    SkillsTarget = new List<string> { "C#" }
                },
                Current = new SurveyCurrentDto
                {
                    Skills = new List<SurveySkillLevelDto>
                    {
                        new SurveySkillLevelDto { Skill = "C#", Level = "Basic", SfiaLevel = 1 }
                    }
                },
                Gap = new SurveyGapDto
                {
                    Missing = new List<string>(),
                    Weak = new List<string> { "C#" }
                }
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, processResponse.StatusCode, "Snapshot seed without roadmap returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task ViewPersonalizedRoadmap_ReturnsSuccess_WhenRoadmapExists()
        {
            var userId = Guid.NewGuid();
            await SeedSnapshotWithRoadmapAsync(userId);

            var response = await _api.GetAsync($"/api/v1/assessment/roadmap/{userId}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Existing roadmap returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task ViewPersonalizedRoadmap_NoSnapshot_ReturnsNotFound()
        {
            var response = await _api.GetAsync($"/api/v1/assessment/roadmap/{Guid.NewGuid()}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Missing snapshot returns 404 NotFound");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task ViewPersonalizedRoadmap_EmptyUserId_ReturnsNotFound()
        {
            var response = await _api.GetAsync($"/api/v1/assessment/roadmap/{Guid.Empty}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Empty userId returns 404 NotFound");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task ViewPersonalizedRoadmap_InvalidRouteUserId_ReturnsNotFound()
        {
            var response = await _api.GetAsync("/api/v1/assessment/roadmap/not-a-guid", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Invalid route userId returns 404 NotFound");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task ViewPersonalizedRoadmap_SnapshotWithoutRoadmap_ReturnsOk()
        {
            var userId = Guid.NewGuid();
            await SeedSnapshotWithoutRoadmapAsync(userId);

            var response = await _api.GetAsync($"/api/v1/assessment/roadmap/{userId}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Snapshot without roadmap returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task ViewPersonalizedRoadmap_RepeatedRead_ReturnsSuccess()
        {
            var userId = Guid.NewGuid();
            await SeedSnapshotWithRoadmapAsync(userId);

            var firstResponse = await _api.GetAsync($"/api/v1/assessment/roadmap/{userId}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, firstResponse.StatusCode, "First read returns 200 OK");

            var response = await _api.GetAsync($"/api/v1/assessment/roadmap/{userId}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Second read returns 200 OK");
        }
    }
}


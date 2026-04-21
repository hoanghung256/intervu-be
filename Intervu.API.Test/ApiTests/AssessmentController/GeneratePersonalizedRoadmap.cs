using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Assessment;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AssessmentController
{
    public class GeneratePersonalizedRoadmapTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public GeneratePersonalizedRoadmapTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
                    TargetLevel = "Junior",
                    TotalPhases = 1
                },
                Phases = new List<SurveyRoadmapPhaseDto>
                {
                    new SurveyRoadmapPhaseDto
                    {
                        PhaseId = "phase-1",
                        PhaseName = "Core Foundations",
                        Nodes = new List<SurveyRoadmapNodeDto>
                        {
                            new SurveyRoadmapNodeDto
                            {
                                SkillId = "skill-csharp",
                                SkillName = "C#",
                                Assessment = new SurveyRoadmapNodeAssessmentDto
                                {
                                    CurrentLevel = "Basic",
                                    TargetLevel = "Intermediate",
                                    SfiaLevel = 2,
                                    Status = "Weak",
                                    Progress = 30
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
                AssessmentName = "Roadmap Seed Assessment",
                Responses = new List<ResponseItem>
                {
                    new ResponseItem { Phase = "phase-1", Skill = "C#", SelectedLevel = "basic" }
                },
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
                },
                Roadmap = BuildRoadmapSeed()
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, processResponse.StatusCode, "Snapshot seed returns 200 OK");
        }

        private async Task SeedIncompleteSnapshotAsync(Guid userId)
        {
            var processResponse = await _api.PostAsync("/api/v1/assessment/process", new SurveyResponsesDto
            {
                UserId = userId,
                AssessmentName = "Incomplete Snapshot"
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, processResponse.StatusCode, "Incomplete snapshot seed returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task GeneratePersonalizedRoadmap_ReturnsSuccess_WhenRoadmapAlreadyExists()
        {
            var userId = Guid.NewGuid();
            await SeedSnapshotWithRoadmapAsync(userId);

            var response = await _api.PostAsync("/api/v1/assessment/roadmap/generate", new GenerateRoadmapFromSurveyRequestDto
            {
                UserId = userId,
                ForceRegenerate = false
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Existing roadmap returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task GeneratePersonalizedRoadmap_RepeatedRequest_ReturnsSuccess()
        {
            var userId = Guid.NewGuid();
            await SeedSnapshotWithRoadmapAsync(userId);

            var firstResponse = await _api.PostAsync("/api/v1/assessment/roadmap/generate", new GenerateRoadmapFromSurveyRequestDto
            {
                UserId = userId,
                ForceRegenerate = false
            }, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, firstResponse.StatusCode, "First generate request returns 200 OK");

            var response = await _api.PostAsync("/api/v1/assessment/roadmap/generate", new GenerateRoadmapFromSurveyRequestDto
            {
                UserId = userId,
                ForceRegenerate = false
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Second generate request returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task GeneratePersonalizedRoadmap_EmptyUserId_ReturnsBadRequest()
        {
            var response = await _api.PostAsync("/api/v1/assessment/roadmap/generate", new GenerateRoadmapFromSurveyRequestDto
            {
                UserId = Guid.Empty,
                ForceRegenerate = false
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Empty userId returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task GeneratePersonalizedRoadmap_NoSnapshot_ReturnsBadRequest()
        {
            var response = await _api.PostAsync("/api/v1/assessment/roadmap/generate", new GenerateRoadmapFromSurveyRequestDto
            {
                UserId = Guid.NewGuid(),
                ForceRegenerate = false
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing snapshot returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task GeneratePersonalizedRoadmap_IncompleteSnapshot_ReturnsBadRequest()
        {
            var userId = Guid.NewGuid();
            await SeedIncompleteSnapshotAsync(userId);

            var response = await _api.PostAsync("/api/v1/assessment/roadmap/generate", new GenerateRoadmapFromSurveyRequestDto
            {
                UserId = userId,
                ForceRegenerate = false
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Incomplete snapshot returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Assessment")]
        public async Task GeneratePersonalizedRoadmap_MissingUserIdField_ReturnsBadRequest()
        {
            var response = await _api.PostAsync("/api/v1/assessment/roadmap/generate", new
            {
                ForceRegenerate = false
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing userId field returns 400 BadRequest");
        }
    }
}


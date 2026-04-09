using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Question;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    // IC-51
    public class ReportQuestionTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ReportQuestionTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task ManageQuestionReports_AdminFlow_ReturnsSuccess()
        {
            var adminLogin = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var adminData = await _api.LogDeserializeJson<LoginResponse>(adminLogin);

            var listReportsResponse = await _api.GetAsync("/api/v1/questions/reports", jwtToken: adminData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, listReportsResponse.StatusCode, "Admin get reports status code is 200 OK");

            var listReportsData = await _api.LogDeserializeJson<PagedResult<QuestionReportItemDto>>(listReportsResponse);
            if (listReportsData.Data!.Items.Any())
            {
                var reportId = listReportsData.Data.Items.First().Id;
                var updateResponse = await _api.PutAsync($"/api/v1/questions/reports/{reportId}/status", new UpdateQuestionReportStatusRequest
                {
                    Status = QuestionReportStatus.Reviewed
                }, jwtToken: adminData.Data.Token, logBody: true);
                await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update report status code is 200 OK");
            }
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetQuestionReports_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var userData = await _api.LogDeserializeJson<LoginResponse>(login);

            var response = await _api.GetAsync("/api/v1/questions/reports", jwtToken: userData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Non-admin user receives 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task UpdateReportStatus_NonExistentReport_ReturnsNotFound()
        {
            var adminLogin = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var adminData = await _api.LogDeserializeJson<LoginResponse>(adminLogin);

            var response = await _api.PutAsync($"/api/v1/questions/reports/{Guid.NewGuid()}/status", new UpdateQuestionReportStatusRequest
            {
                Status = QuestionReportStatus.Reviewed
            }, jwtToken: adminData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status 404 for non-existent report ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetQuestionReports_InvalidPage_ReturnsBadRequest()
        {
            var adminLogin = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var adminData = await _api.LogDeserializeJson<LoginResponse>(adminLogin);

            var response = await _api.GetAsync("/api/v1/questions/reports?page=0&pageSize=10", jwtToken: adminData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status 400 for invalid page number");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task UpdateReportStatus_InvalidStatus_ReturnsBadRequest()
        {
            var adminLogin = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var adminData = await _api.LogDeserializeJson<LoginResponse>(adminLogin);

            var response = await _api.PutAsync($"/api/v1/questions/reports/{Guid.NewGuid()}/status", new
            {
                Status = 999 // Invalid status enum value
            }, jwtToken: adminData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status 400 for invalid status value");
        }
    }
}

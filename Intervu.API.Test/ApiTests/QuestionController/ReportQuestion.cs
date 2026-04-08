using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Question;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
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
    }
}

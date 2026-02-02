using Intervu.API.Test.Base;
using Intervu.API.Test.Pages;
using Intervu.API.Test.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Intervu.API.Test.E2ETests;

public class LoginTests : BaseAutomationTest
{
    public LoginTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Automation")]
    public async Task Should_Display_Error_Message_For_Invalid_Credentials()
    {
        // Arrange: Create a Page Object instance
        var loginPage = new LoginPage(Page);
        
        // Act: Perform actions using the Page Object
        LogInfo("Navigating to the login page.");
        await loginPage.NavigateAsync();
        
        LogInfo("Attempting to log in with invalid credentials.");
        await loginPage.LoginAsync("wrong@user.com", "wrongpassword");

        // Assert: Verify the outcome
        LogInfo("Verifying that an error message is displayed.");
        await loginPage.VerifyToastMessageAsync("Invalid email or password");
    }

    [Fact]
    [Trait("Category", "Automation")]
    [Trait("Category", "Smoke")]
    public async Task Should_Redirect_To_Dashboard_After_Successful_Login()
    {
        // Arrange
        var loginPage = new LoginPage(Page);
        // You would typically get valid credentials from a secure source or config
        var validEmail = Environment.GetEnvironmentVariable("TEST_USER_EMAIL") ?? "alice@example.com";
        var validPassword = Environment.GetEnvironmentVariable("TEST_USER_PASSWORD") ?? "123";

        // Act
        LogInfo("Navigating to the login page.");
        await loginPage.NavigateAsync();

        LogInfo("Logging in with valid credentials.");
        await loginPage.LoginAsync(validEmail, validPassword);

        // Assert: Verify the outcome
        LogInfo("Verifying that a success message is displayed.");
        await loginPage.VerifyToastMessageAsync("Successful");

        // Assert
        LogInfo("Verifying redirection to the homepage.");
        // After a successful login, the URL should change. We wait for it to match.
        await Page.WaitForURLAsync("**/home"); // Using a glob pattern
        await AssertHelper.AssertContains("/home", Page.Url, "Successfully redirected to the homepage.");
    }

    [Fact]
    [Trait("Category", "Automation")]
    public async Task Should_Display_Error_Message_For_Invalid_Email()
    {
        // Arrange: Create a Page Object instance
        var loginPage = new LoginPage(Page);

        // Act: Perform actions using the Page Object
        LogInfo("Navigating to the login page.");
        await loginPage.NavigateAsync();

        LogInfo("Attempting to log in with invalid credentials.");
        await loginPage.LoginAsync("wrong", "wrongpassword");

        // Assert: Verify the outcome
        LogInfo("Verifying that an error message is displayed.");
        await loginPage.VerifyToastMessageAsync("Invalid email or password");
    }
}
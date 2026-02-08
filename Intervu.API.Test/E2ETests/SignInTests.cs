using Intervu.API.Test.Base;
using Intervu.API.Test.Pages;
using Intervu.API.Test.Pages.Authentication;
using Intervu.API.Test.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Intervu.API.Test.E2ETests;

public class SignInTests : BaseAutomationTest
{
    public SignInTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Automation")]
    public async Task Should_Display_Error_Message_For_Invalid_Credentials()
    {
        // Arrange: Create a Page Object instance
        var signInPage = new SignInPage(Page);
        
        // Act: Perform actions using the Page Object
        LogInfo("Navigating to the login page.");
        await signInPage.NavigateAsync();
        
        LogInfo("Attempting to log in with invalid credentials.");
        await signInPage.SignInAsync("wrong@user.com", "wrongpassword");

        // Assert: Verify the outcome
        LogInfo("Verifying that an error message is displayed.");
        await signInPage.VerifyErrorToastMessageAsync("Invalid email or password");
    }

    [Fact]
    [Trait("Category", "Automation")]
    [Trait("Category", "Smoke")]
    public async Task Should_Redirect_To_Dashboard_After_Successful_Login()
    {
        // Arrange
        var signInPage = new SignInPage(Page);
        // You would typically get valid credentials from a secure source or config
        var validEmail = Environment.GetEnvironmentVariable("TEST_USER_EMAIL") ?? "alice@example.com";
        var validPassword = Environment.GetEnvironmentVariable("TEST_USER_PASSWORD") ?? "123";

        // Act
        LogInfo("Navigating to the login page.");
        await signInPage.NavigateAsync();

        LogInfo("Logging in with valid credentials.");
        await signInPage.SignInAsync(validEmail, validPassword);

        // Assert: Verify the outcome
        LogInfo("Verifying that a success message is displayed.");
        await signInPage.VerifySuccessToastMessageAsync("Successful");

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
        var signInPage = new SignInPage(Page);

        // Act: Perform actions using the Page Object
        LogInfo("Navigating to the login page.");
        await signInPage.NavigateAsync();

        LogInfo("Attempting to log in with invalid credentials.");
        await signInPage.SignInAsync("wrong", "wrongpassword");

        // Assert: Verify the outcome
        LogInfo("Verifying that an error message is displayed.");
        await signInPage.VerifyErrorToastMessageAsync("Invalid email or password");
    }

    [Fact]
    [Trait("Category", "Automation")]
    public async Task Should_Navigate_To_SignUp_Page()
    {
        // Arrange
        var signInPage = new SignInPage(Page);

        // Act
        LogInfo("Navigating to the login page.");
        await signInPage.NavigateAsync();
        LogInfo("Clicking on the Sign Up link.");
        await signInPage.NavigateToSignUpPageAsync();

        // Assert
        LogInfo("Verifying redirection to the Sign Up page.");
        // Assuming the URL contains 'signup' or similar. Adjust the string if your route is different.
        await AssertHelper.AssertContains("signup", Page.Url, "Navigated to Sign Up page.");
    }

    [Fact]
    [Trait("Category", "Automation")]
    public async Task Should_Navigate_To_ForgotPassword_Page()
    {
        // Arrange
        var signInPage = new SignInPage(Page);

        // Act
        LogInfo("Navigating to the login page.");
        await signInPage.NavigateAsync();
        LogInfo("Clicking on the Forgot Password link.");
        await signInPage.NavigateToForgotPasswordPageAsync();

        // Assert
        LogInfo("Verifying redirection to the Forgot Password page.");
        // Assuming the URL contains 'forgot-password' or similar.
        await AssertHelper.AssertContains("forgot-password", Page.Url, "Navigated to Forgot Password page.");
    }

    // [Fact]
    // [Trait("Category", "Automation")]
    // public async Task Should_Display_Error_Message_For_SqlInjection_Attempt()
    // {
    //     // Arrange: Create a Page Object instance
    //     var signInPage = new SignInPage(Page);
    //
    //     // Act: Perform actions using the Page Object
    //     LogInfo("Navigating to the login page.");
    //     await signInPage.NavigateAsync();
    //
    //     LogInfo("Attempting to log in with SQL Injection payload.");
    //     await signInPage.SignInAsync("' OR '1'='1", "' OR '1'='1");
    //
    //     // Assert: Verify the outcome
    //     LogInfo("Verifying that an error message is displayed.");
    //     await signInPage.VerifyErrorToastMessageAsync("Invalid email or password");
    // }
}
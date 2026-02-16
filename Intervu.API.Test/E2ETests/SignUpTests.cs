using Intervu.API.Test.Base;
using Intervu.API.Test.Pages.Authentication;
using Intervu.API.Test.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Intervu.API.Test.E2ETests;

public class SignUpTests : BaseAutomationTest
{
    public SignUpTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Automation")]
    public async Task Should_Navigate_To_SignIn_Page_From_SignUp_Page()
    {
        // Arrange
        var signInPage = new SignInPage(Page);
        var signUpPage = new SignUpPage(Page);

        // Act
        LogInfo("Navigating to the login page.");
        await signInPage.NavigateAsync();
        
        LogInfo("Navigating to the sign-up page.");
        await signInPage.NavigateToSignUpPageAsync();

        LogInfo("Clicking on Sign In link.");
        await signUpPage.NavigateToSignInPageAsync();

        // Assert
        LogInfo("Verifying redirection back to the Sign In page.");
        await AssertHelper.AssertContains("/login", Page.Url, "Navigated back to Sign In page.");
    }

    [Fact]
    [Trait("Category", "Automation")]
    [Trait("Category", "Smoke")]
    public async Task Should_Register_User_Successfully()
    {
        // Arrange
        var signInPage = new SignInPage(Page);
        var signUpPage = new SignUpPage(Page);
        
        // Generate random credentials to ensure registration success
        string randomId = Guid.NewGuid().ToString("N").Substring(0, 8);
        string fullName = $"Test User {randomId}";
        string email = $"test.user.{randomId}@example.com";
        string password = "Password123!";

        // Act
        LogInfo("Navigating to the sign-up page.");
        await signInPage.NavigateAsync();
        await signInPage.NavigateToSignUpPageAsync();

        LogInfo($"Attempting to register with email: {email}");
        await signUpPage.SignUpAsync(fullName, email, password);

        // Assert
        LogInfo("Verifying success message.");
        await signUpPage.VerifySuccessToastMessageAsync("Registration Successful");
    }

    [Fact]
    [Trait("Category", "Automation")]
    public async Task Should_Display_Error_When_Registering_With_Invalid_Email()
    {
        // Arrange
        var signInPage = new SignInPage(Page);
        var signUpPage = new SignUpPage(Page);

        // Act
        LogInfo("Navigating to the sign-up page.");
        await signInPage.NavigateAsync();
        await signInPage.NavigateToSignUpPageAsync();

        LogInfo("Attempting to register with invalid email.");
        await signUpPage.SignUpAsync("Test User", "invalid-email-format", "Password123!");

        // Assert
        LogInfo("Verifying error message.");
        await signUpPage.VerifyEmailErrorLabelAsync();
    }

    [Fact]
    [Trait("Category", "Automation")]
    public async Task Should_Display_Error_When_Registering_With_Existing_Email()
    {
        // Arrange
        var signInPage = new SignInPage(Page);
        var signUpPage = new SignUpPage(Page);

        // Generate random credentials
        string randomId = Guid.NewGuid().ToString("N").Substring(0, 8);
        string fullName = $"Test User {randomId}";
        string email = $"test.user.{randomId}@example.com";
        string password = "Password123!";

        // Act - First Registration
        LogInfo("Navigating to the sign-up page.");
        await signInPage.NavigateAsync();
        await signInPage.NavigateToSignUpPageAsync();

        LogInfo($"Registering user with email: {email}");
        await signUpPage.SignUpAsync(fullName, email, password);
        await signUpPage.VerifySuccessToastMessageAsync("Registration Successful");

        // Act - Second Registration
        LogInfo("Navigating back to sign-up page.");
        await signInPage.NavigateAsync();
        await signInPage.NavigateToSignUpPageAsync();

        LogInfo("Attempting to register with the same email.");
        await signUpPage.SignUpAsync(fullName, email, password);

        // Assert
        LogInfo("Verifying error message.");
        await signUpPage.VerifyErrorToastMessageAsync("Registration failed. Email may already exist.");
    }

    [Fact]
    [Trait("Category", "Automation")]
    public async Task Should_Display_Error_When_Registering_With_Weak_Password()
    {
        // Arrange
        var signInPage = new SignInPage(Page);
        var signUpPage = new SignUpPage(Page);

        // Act
        LogInfo("Navigating to the sign-up page.");
        await signInPage.NavigateAsync();
        await signInPage.NavigateToSignUpPageAsync();

        LogInfo("Attempting to register with weak password.");
        await signUpPage.SignUpAsync("Test User", "weak.password.test@example.com", "123");

        // Assert
        LogInfo("Verifying error message.");
        await signUpPage.VerifyPasswordErrorLabelAsync("Password");
    }

    [Fact]
    [Trait("Category", "Automation")]
    public async Task Should_Display_Error_When_Registering_With_Very_Short_Password()
    {
        // Arrange
        var signInPage = new SignInPage(Page);
        var signUpPage = new SignUpPage(Page);

        // Act
        LogInfo("Navigating to the sign-up page.");
        await signInPage.NavigateAsync();
        await signInPage.NavigateToSignUpPageAsync();

        LogInfo("Attempting to register with short password.");
        await signUpPage.SignUpAsync("Test User", "short.pass@example.com", "1");

        // Assert
        LogInfo("Verifying error message.");
        await signUpPage.VerifyPasswordErrorLabelAsync("Password");
    }
}
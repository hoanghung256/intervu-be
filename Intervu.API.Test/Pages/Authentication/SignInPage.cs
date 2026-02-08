using Intervu.API.Test.Controls;
using Intervu.API.Test.Utils;
using Microsoft.Playwright;

namespace Intervu.API.Test.Pages.Authentication;

public class SignInPage : BasePage
{
    private readonly TextField _emailInput;
    private readonly TextField _passwordInput;
    private readonly Button _signInButton;
    private readonly Link _forgotPasswordLink;
    private readonly Link _signUpLink;
    private readonly Toast _toastMessage;

    public SignInPage(IPage page) : base(page)
    {
        _emailInput = new TextField(Page, "input[type='email']");
        _passwordInput = new TextField(Page, "input[type='password']");
        _signInButton = new Button(Page, "button[type='submit']");
        _forgotPasswordLink = new Link(Page, "//p[normalize-space(.)='Forgot password?']");
        _signUpLink = new Link(Page, "//span[normalize-space(.)='Sign up']");
        _toastMessage = new Toast(Page);
    }

    /// <summary>
    /// Navigates to the sign-in page.
    /// </summary>
    public async Task NavigateAsync()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
    }
    
    /// <summary>
    /// Navigates to the sign-up page.
    /// </summary>
    public async Task NavigateToSignUpPageAsync()
    {
        await _signUpLink.ClickAsync();
    }
    
    /// <summary>
    /// Navigates to the forgot password page.
    /// </summary>
    public async Task NavigateToForgotPasswordPageAsync()
    {
        await _forgotPasswordLink.ClickAsync();
    }

    /// <summary>
    /// Fills the sign-in form and submits it.
    /// </summary>
    /// <param name="email">User's email.</param>
    /// <param name="password">User's password.</param>
    public async Task SignInAsync(string email, string password)
    {
        await _emailInput.FillAsync(email);
        await _passwordInput.FillAsync(password);
        await _signInButton.ClickAsync();
    }

    /// <summary>
    /// Verifies that the success toast notification with the specified message appears.
    /// </summary>
    public async Task VerifySuccessToastMessageAsync(string message)
    {
        await AssertHelper.AssertToastMessageAsync(_toastMessage, message, "Toast message is displayed with correct text.");
        await AssertHelper.AssertToastSuccessAsync(_toastMessage, "Toast success mark is displayed.");
    }

    /// <summary>
    /// Verifies that the error toast notification with the specified message appears.
    /// </summary>
    public async Task VerifyErrorToastMessageAsync(string message)
    {
        await AssertHelper.AssertToastMessageAsync(_toastMessage, message, "Toast message is displayed with correct text.");
        await AssertHelper.AssertToastErrorAsync(_toastMessage, "Toast error mark is displayed.");
    }
}
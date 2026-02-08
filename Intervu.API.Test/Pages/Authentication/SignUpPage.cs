using Intervu.API.Test.Controls;
using Intervu.API.Test.Utils;
using Microsoft.Playwright;

namespace Intervu.API.Test.Pages.Authentication;

public class SignUpPage : BasePage
{
    private readonly TextField _fullNameInput;
    private readonly TextField _emailInput;
    private readonly Label _emailErrorLabel;
    private readonly TextField _passwordInput;
    private readonly Label _passwordErrorLabel;
    private readonly Button _signUpButton;
    private readonly Link _signInLink;
    private readonly Toast _toastMessage;
    
    public SignUpPage(IPage page) : base(page)
    {
        _fullNameInput = new TextField(Page, "input[name='fullName']");
        _emailInput = new TextField(Page, "input[name='email']");
        _emailErrorLabel = new Label(Page, "//p[normalize-space(.)='Invalid email format']");
        _passwordInput = new TextField(Page, "input[name='password']");
        _passwordErrorLabel = new Label(Page, "//p[contains(normalize-space(.), 'Password')]");
        _signUpButton = new Button(Page, "button[type='submit']");
        _signInLink = new Link(Page, "//span[normalize-space(.)='Sign in']");
        _toastMessage = new Toast(Page);
    }
    
    /// <summary>
    /// Navigates to the sign-in page.
    /// </summary>
    public async Task NavigateToSignInPageAsync()
    {
        await _signInLink.ClickAsync();
    }
    
    /// <summary>
    /// Fills the sign-up form and submits it.
    /// </summary>
    public async Task SignUpAsync(string fullName, string email, string password)
    {
        await _fullNameInput.FillAsync(fullName);
        await _emailInput.FillAsync(email);
        await _passwordInput.FillAsync(password);
        await _signUpButton.ClickAsync();
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
    
    /// <summary>
    /// Verifies that the error label for the email field appears.
    /// </summary>
    public async Task VerifyEmailErrorLabelAsync()
    {
        await AssertHelper.AssertTrue(await _emailErrorLabel.IsVisibleAsync(), "Email error label is displayed.");
    }

    /// <summary>
    /// Verifies that the password error label is displayed with the specified message.
    /// </summary>
    public async Task VerifyPasswordErrorLabelAsync(string message)
    {
        await AssertHelper.AssertTrue(await _passwordErrorLabel.IsVisibleAsync(), "Password error label is displayed.");
        string? actualText = await _passwordErrorLabel.TextContentAsync();
        await AssertHelper.AssertContains(message, actualText ?? string.Empty, "Password error label text is correct.");
    }
}
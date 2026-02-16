using Intervu.API.Test.Controls;
using Intervu.API.Test.Utils;
using Microsoft.Playwright;

namespace Intervu.API.Test.Pages.Authentication;

public class ForgotPasswordPage: BasePage
{
    private readonly TextField _emailInput;
    private readonly Button _sendResetPasswordButton;
    private readonly Link _backToSignInLink;
    private readonly Toast _toastMessage;
    
    public ForgotPasswordPage(IPage page) : base(page)
    {
        _emailInput = new TextField(Page, "input[type='email']");
        _sendResetPasswordButton = new Button(Page, "button[type='submit']");
        _backToSignInLink = new Link(Page, "//p[normalize-space(.)='Back to Login']");
        _toastMessage = new Toast(Page);
    }
    
    /// <summary>
    /// Navigates to the sign-in page.
    /// </summary>
    public async Task NavigateToSignInPageAsync()
    {
        await _backToSignInLink.ClickAsync();
    }
    
    /// <summary>
    /// Sends a reset password email to the specified email address.
    /// </summary>
    public async Task SendResetPasswordEmailAsync(string email)
    {
        await _emailInput.FillAsync(email);
        await _sendResetPasswordButton.ClickAsync();
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
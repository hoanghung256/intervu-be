using Intervu.API.Test.Controls;
using Microsoft.Playwright;
using Intervu.API.Test.Utils;

namespace Intervu.API.Test.Pages;

public class LoginPage : BasePage
{
    // Controls for elements on the page, abstracting away ILocator details.
    private readonly TextField _emailInput;
    private readonly TextField _passwordInput;
    private readonly Button _loginButton;
    private readonly Toast _toastMessage;

    public LoginPage(IPage page) : base(page)
    {
        // It's best to use test-ids, but for this example, we'll use common selectors.
        _emailInput = new TextField(Page, "input[type='email']");
        _passwordInput = new TextField(Page, "input[type='password']");
        _loginButton = new Button(Page, "button[type='submit']");
        _toastMessage = new Toast(Page);
    }

    /// <summary>
    /// Navigates to the login page.
    /// </summary>
    public async Task NavigateAsync()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
    }

    /// <summary>
    /// Fills the login form and submits it.
    /// </summary>
    /// <param name="email">User's email.</param>
    /// <param name="password">User's password.</param>
    public async Task LoginAsync(string email, string password)
    {
        await _emailInput.FillAsync(email);
        await _passwordInput.FillAsync(password);
        await _loginButton.ClickAsync();
    }

    /// <summary>
    /// Verifies that a toast notification with the specified message appears.
    /// </summary>
    public Task VerifyToastMessageAsync(string message)
    {
        return AssertHelper.AssertToastMessageAsync(_toastMessage, message, "Toast message is displayed with correct text.");
    }
}
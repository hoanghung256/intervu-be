using Microsoft.Playwright;

namespace Intervu.API.Test.Pages;

public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly string BaseUrl;

    protected BasePage(IPage page)
    {
        Page = page;
        // It's a good practice to fetch the base URL from an environment variable
        // to easily switch between local, staging, and production environments.
        BaseUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173";
    }

    public async Task<string> GetTitleAsync()
    {
        return await Page.TitleAsync();
    }
}
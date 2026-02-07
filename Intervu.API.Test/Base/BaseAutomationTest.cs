using Microsoft.Playwright;
using Xunit.Abstractions;

namespace Intervu.API.Test.Base;

public class BaseAutomationTest : BaseTest, IAsyncLifetime
{
    // Shared Playwright/Browser instances (Singleton across all tests)
    private static readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
    private static IPlaywright _playwright;
    private static IBrowser _browser;

    // Per-test isolated instances
    protected IBrowserContext Context { get; private set; }
    protected IPage Page { get; private set; }
    
    public BaseAutomationTest(ITestOutputHelper output) : base(output)
    {
    }

    static BaseAutomationTest()
    {
        // Ensure Playwright browsers are installed when the factory type is first accessed
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
        if (exitCode != 0)
        {
            throw new System.Exception($"Playwright install failed with exit code {exitCode}");
        }
    }
    
    /// <summary>
    /// Called immediately after the class has been created, before the test method is called.
    /// Initializes Playwright (once) and creates a fresh Context/Page (per test).
    /// </summary>
    public async Task InitializeAsync()
    {
        ScreenshotProvider = async () => await Page.ScreenshotAsync();
        // Ensure BaseTest initializes the Extent Report test node
        await base.InitializeAsync();

        // 1. Lazy initialization of the shared Browser instance
        if (_browser == null)
        {
            await _initLock.WaitAsync();
            try
            {
                if (_playwright == null)
                {
                    _playwright = await Playwright.CreateAsync();
                }
                if (_browser == null)
                {
                    bool headless = false;

                    var env = Environment.GetEnvironmentVariable("PLAYWRIGHT_HEADLESS");
                    if (!string.IsNullOrEmpty(env) && bool.TryParse(env, out var parsed))
                    {
                        headless = parsed;
                    }

                    _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                       {
                           Headless = headless
                       });
                }
            }
            finally
            {
                _initLock.Release();
            }
        }

        // 2. Create a new isolated context and page for this specific test
        Context = await _browser.NewContextAsync();
        Page = await Context.NewPageAsync();
        
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173/";

        LogInfo($"Navigating to {frontendUrl}");
        
        try 
        {
            await Page.GotoAsync(frontendUrl, new PageGotoOptions { Timeout = 30000 });
            var title = await Page.TitleAsync();
            await LogPass($"Page loaded. Title: {title}");
        }
        catch (TimeoutException)
        {
            await LogFail("Could not connect to frontend (expected if not running). Skipping assertion.");
        }
        catch (Exception ex)
        {
            await LogFail($"Exception during navigation: {ex.Message}");
        }
    }

    public async Task DisposeAsync()
    {
        // Close the page and context to clean up resources for this test
        if (Page != null) await Page.CloseAsync();
        if (Context != null) await Context.CloseAsync();

        // Ensure BaseTest flushes/saves the Extent Report data
        await base.DisposeAsync();
    }
}
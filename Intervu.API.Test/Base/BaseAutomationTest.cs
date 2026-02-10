using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace Intervu.API.Test.Base;

// Forces all tests inheriting from this class to run sequentially in the same collection.
// This prevents race conditions on the single Browser instance and reduces memory pressure.
[Collection("Automation Collection")]
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

    /// <summary>
    /// Called immediately after the class has been created, before the test method is called.
    /// Initializes Playwright (once) and creates a fresh Context/Page (per test).
    /// </summary>
    public async Task InitializeAsync()
    {
        ScreenshotProvider = async () => await Page.ScreenshotAsync();
        // Ensure BaseTest initializes the Extent Report test node
        await base.InitializeAsync();

        // 0. Self-Healing: Check if the shared browser instance has crashed or disconnected.
        // If it has, dispose of it so we can create a fresh one.
        if (_browser != null && !_browser.IsConnected)
        {
            try { await _browser.DisposeAsync(); } catch { }
            _browser = null;
            // We don't null _playwright as the process usually survives, but we could if needed.
        }

        // 1. Lazy initialization of the shared Browser instance
        if (_browser == null)
        {
            // Add a generous timeout (60s) to prevent deadlocks during slow CI startups
            if (!await _initLock.WaitAsync(TimeSpan.FromSeconds(60)))
            {
                throw new TimeoutException("Timed out waiting for browser initialization lock.");
            }
            try
            {
                if (_playwright == null)
                {
                    // Move installation here to ensure it's thread-safe and only runs when needed
                    var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
                    if (exitCode != 0)
                    {
                        throw new Exception($"Playwright install failed with exit code {exitCode}");
                    }
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
                           Headless = headless,
                           Args = new[] 
                           { 
                               "--disable-dev-shm-usage", // Crucial for Docker/CI to prevent OOM/Crashes
                               "--no-sandbox",            // Reduces memory overhead
                               "--disable-gpu",           // Disables GPU hardware acceleration
                               "--disable-extensions",    // Disables extensions to save memory
                               "--disable-setuid-sandbox" // Additional stability for Docker
                           }
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
        try
        {
            if (Page != null) await Page.CloseAsync();
        }
        catch (Exception) { /* Ignore cleanup errors to prevent masking actual test failures */ }

        try
        {
            if (Context != null) await Context.CloseAsync();
        }
        catch (Exception) { /* Ignore cleanup errors */ }

        // Ensure BaseTest flushes/saves the Extent Report data
        await base.DisposeAsync();
    }
}
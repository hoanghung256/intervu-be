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
        try
        {
            // Ensure BaseTest initializes the Extent Report test node
            await base.InitializeAsync();

            // 0. Self-Healing: Check if the shared browser instance has crashed or disconnected.
            if (_browser != null && !_browser.IsConnected)
            {
                try { await _browser.DisposeAsync(); } catch { }
                _browser = null;
            }

            // 1. Lazy initialization of the shared Browser instance
            if (_browser == null)
            {
                if (!await _initLock.WaitAsync(TimeSpan.FromSeconds(60)))
                {
                    throw new TimeoutException("Timed out waiting for browser initialization lock.");
                }
                try
                {
                    if (_playwright == null)
                    {
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
                                "--disable-dev-shm-usage",
                                "--no-sandbox",
                                "--disable-gpu",
                                "--disable-extensions",
                                "--disable-setuid-sandbox"
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
            ScreenshotProvider = async () => await Page.ScreenshotAsync();

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
        }
        catch (Exception ex)
        {
            await LogFail("Failed during automation test initialization", ex);
            throw; // Re-throw so xUnit knows the test failed
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (Page != null) await Page.CloseAsync();
            if (Context != null) await Context.CloseAsync();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"[WARN] Error during automation cleanup: {ex.Message}");
        }

        // Ensure BaseTest flushes/saves the Extent Report data
        await base.DisposeAsync();
    }
}

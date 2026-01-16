using Intervu.API.Test.Base;
using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace Intervu.API.Test.Controls
{
    /// <summary>
    /// The base class for all UI control abstractions.
    /// It wraps an ILocator and provides common functionality.
    /// </summary>
    public abstract class BaseControl
    {
        protected ILocator Locator { get; }
        protected IPage Page { get; }
        public string Selector { get; }

        protected BaseControl(IPage page, string selector)
        {
            Page = page;
            Locator = page.Locator(selector);
            Selector = selector;
        }

        protected async Task LogActionAsync(string actionName, Func<Task> action)
        {
            var test = BaseTest.Current.Value;
            string stepName = $"{actionName} [{Selector}]";
            if (test != null)
            {
                await test.LogStepAsync(stepName, action);
            }
            else
            {
                await action();
            }
        }

        protected async Task<T> LogActionAsync<T>(string actionName, Func<Task<T>> action)
        {
            var test = BaseTest.Current.Value;
            string stepName = $"{actionName} [{Selector}]";
            if (test != null)
            {
                return await test.LogStepAsync(stepName, action);
            }
            else
            {
                return await action();
            }
        }

        public Task<bool> IsVisibleAsync() => LogActionAsync("Check Visibility", () => Locator.IsVisibleAsync());

        public Task ClickAsync(LocatorClickOptions? options = null) => LogActionAsync("Click", () => Locator.ClickAsync(options));

        public Task<string?> TextContentAsync() => LogActionAsync("Get Text Content", () => Locator.TextContentAsync());

        public Task HighlightAsync() => LogActionAsync("Highlight", () => Locator.HighlightAsync());

        public Task<string?> GetAttributeAsync(string name, LocatorGetAttributeOptions? options = null) => LogActionAsync($"Get Attribute '{name}'", () => Locator.GetAttributeAsync(name, options));

        // --- Additional Methods wrapping Page/Locator functionality with logging ---

        public Task WaitForAsync(LocatorWaitForOptions? options = null) => LogActionAsync("Wait For Element", () => Locator.WaitForAsync(options));

        public Task ScrollIntoViewAsync(LocatorScrollIntoViewIfNeededOptions? options = null) => LogActionAsync("Scroll Into View", () => Locator.ScrollIntoViewIfNeededAsync(options));

        public Task ScreenshotAsync(LocatorScreenshotOptions? options = null) => LogActionAsync("Capture Screenshot", () => Locator.ScreenshotAsync(options));

        public Task PressAsync(string key, LocatorPressOptions? options = null) => LogActionAsync($"Press Key '{key}'", () => Locator.PressAsync(key, options));
        
        public Task SetInputFilesAsync(FilePayload payload, LocatorSetInputFilesOptions? options = null) => LogActionAsync($"Set Input Files '{payload.Name}'", () => Locator.SetInputFilesAsync(payload, options));
    }
}
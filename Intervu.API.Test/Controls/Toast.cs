using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace Intervu.API.Test.Controls
{
    /// <summary>
    /// Represents a toast notification container.
    /// </summary>
    public class Toast : BaseControl
    {
        // Default selector matches the container: <div data-rht-toaster="" ...>
        public Toast(IPage page, string selector = "div[data-rht-toaster]") : base(page, selector) { }

        /// <summary>
        /// Waits for a specific message to appear inside the toast container.
        /// </summary>
        /// <param name="message">The text to verify.</param>
        public async Task WaitForMessageAsync(string message)
        {
            // Wait for any toast item (child div) to appear in the container.
            // We target the first div, assuming it represents the toast notification wrapper.
            var toastItem = Locator.Locator("div").First;
            await toastItem.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            // Get the text content of the visible toast.
            var actualText = await toastItem.InnerTextAsync();

            // Fail immediately if the text does not match the expectation.
            if (!actualText.Contains(message, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Toast message mismatch. Expected '{message}', but found '{actualText}'.");
            }
        }
    }
}
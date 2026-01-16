using Microsoft.Playwright;
using System.Threading.Tasks;

namespace Intervu.API.Test.Controls
{
    /// <summary>
    /// Represents a text input field or textarea.
    /// </summary>
    public class TextField : BaseControl
    {
        public TextField(IPage page, string selector) : base(page, selector) { }

        public Task FillAsync(string value, LocatorFillOptions? options = null) => LogActionAsync($"Fill '{value}'", () => Locator.FillAsync(value, options));

        public Task ClearAsync(LocatorClearOptions? options = null) => LogActionAsync("Clear Text", () => Locator.ClearAsync(options));

        public Task<string> InputValueAsync(LocatorInputValueOptions? options = null) => LogActionAsync("Get Input Value", () => Locator.InputValueAsync(options));

        public Task TypeAsync(string text, LocatorTypeOptions? options = null) => LogActionAsync($"Type '{text}'", () => Locator.TypeAsync(text, options));
    }
}
using Microsoft.Playwright;

namespace Intervu.API.Test.Controls
{
    /// <summary>
    /// Represents a clickable button.
    /// </summary>
    public class Button : BaseControl
    {
        public Button(IPage page, string selector) : base(page, selector) { }
    }
}
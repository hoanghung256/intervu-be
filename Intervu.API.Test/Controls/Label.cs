using Microsoft.Playwright;

namespace Intervu.API.Test.Controls
{
    /// <summary>
    /// Represents a text label or any element used to display static text.
    /// </summary>
    public class Label : BaseControl
    {
        public Label(IPage page, string selector) : base(page, selector) { }
    }
}
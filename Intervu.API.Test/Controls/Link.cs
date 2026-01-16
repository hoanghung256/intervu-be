using Microsoft.Playwright;
using System.Threading.Tasks;

namespace Intervu.API.Test.Controls
{
    /// <summary>
    /// Represents an anchor (<a>) element.
    /// </summary>
    public class Link : BaseControl
    {
        public Link(IPage page, string selector) : base(page, selector) { }

        public Task<string?> GetHrefAsync() => GetAttributeAsync("href");
    }
}
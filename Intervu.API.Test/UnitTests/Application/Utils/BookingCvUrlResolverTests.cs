using Intervu.Application.Exceptions;
using Intervu.Application.Utils;

namespace Intervu.API.Test.UnitTests.Application.Utils;

public class BookingCvUrlResolverTests
{
    [Fact]
    public void Resolve_RequiresCv_Missing_Throws()
    {
        var ex = Assert.Throws<BadRequestException>(() =>
            BookingCvUrlResolver.Resolve(requiresCandidateCv: true, cvUrl: null));
        Assert.Contains("requires a CV", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_RequiresCv_Empty_Throws()
    {
        Assert.Throws<BadRequestException>(() =>
            BookingCvUrlResolver.Resolve(requiresCandidateCv: true, cvUrl: "   "));
    }

    [Fact]
    public void Resolve_RequiresCv_InvalidUrl_Throws()
    {
        Assert.Throws<BadRequestException>(() =>
            BookingCvUrlResolver.Resolve(requiresCandidateCv: true, cvUrl: "not-a-url"));
    }

    [Fact]
    public void Resolve_RequiresCv_Https_ReturnsTrimmed()
    {
        var url = BookingCvUrlResolver.Resolve(true, "  https://cdn.example.com/cv.pdf  ");
        Assert.Equal("https://cdn.example.com/cv.pdf", url);
    }

    [Fact]
    public void Resolve_NotRequired_NoUrl_ReturnsNull()
    {
        Assert.Null(BookingCvUrlResolver.Resolve(false, null));
    }

    [Fact]
    public void Resolve_NotRequired_WithValidUrl_ReturnsTrimmed()
    {
        var url = BookingCvUrlResolver.Resolve(false, "http://example.com/a.pdf");
        Assert.Equal("http://example.com/a.pdf", url);
    }

    [Fact]
    public void Resolve_NotRequired_InvalidUrl_Throws()
    {
        Assert.Throws<BadRequestException>(() =>
            BookingCvUrlResolver.Resolve(false, cvUrl: "ftp://bad"));
    }
}

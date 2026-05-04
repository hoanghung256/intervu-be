using Intervu.Application.Exceptions;

namespace Intervu.Application.Utils;

/// <summary>
/// Normalizes and validates CV URL for direct booking when an interview type requires it (or when optionally supplied).
/// </summary>
public static class BookingCvUrlResolver
{
    public static string? Resolve(bool requiresCandidateCv, string? cvUrl)
    {
        var trimmed = string.IsNullOrWhiteSpace(cvUrl) ? null : cvUrl.Trim();

        if (requiresCandidateCv)
        {
            if (trimmed is null)
                throw new BadRequestException(
                    "This interview type requires a CV. Upload a resume or select one from your profile.");
            EnsureValidHttpUrl(trimmed);
            return trimmed;
        }

        if (trimmed is null)
            return null;

        EnsureValidHttpUrl(trimmed);
        return trimmed;
    }

    private static void EnsureValidHttpUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new BadRequestException("CV URL must be a valid http or https address.");
        }
    }
}

namespace Intervu.Application.Utils
{
    /// <summary>
    /// Normalizes optional candidate booking notes: trim whitespace, empty → null, enforce max length.
    /// </summary>
    public static class CandidateNoteNormalizer
    {
        public const int MaxLength = 1000;

        public static string? Normalize(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var trimmed = raw.Trim();
            return trimmed.Length <= MaxLength ? trimmed : trimmed[..MaxLength];
        }
    }
}

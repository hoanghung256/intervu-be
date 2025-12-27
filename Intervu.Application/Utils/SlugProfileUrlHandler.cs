using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Intervu.Application.Utils
{
    public class SlugProfileUrlHandler
    {

        public static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var normalized = input.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var category = Char.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            var noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

            var lower = noDiacritics.ToLowerInvariant();

            var cleaned = Regex.Replace(lower, @"[^a-z0-9\s-]", "");

            var slug = Regex.Replace(cleaned, @"[\s-]+", "-").Trim('-');

            return slug;
        }

        public static string GenerateProfileSlug(string fullName)
        {
            var slug = Slugify(fullName);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return $"{slug}_{timestamp}";
        }

    }
}

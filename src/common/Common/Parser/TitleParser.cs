using System.Text.RegularExpressions;

namespace Common.Parser;

public static class TitleParser
{
    public static string ExtractTitle(string input)
    {
        var technicalTags = new[]
        {
            "2160p", "1080p", "720p", "480p", "4K", "WEB", "BluRay", "BDRip", "DVDRip",
            "x265", "x264", "HEVC", "10bit", "8bit", "AAC5", "AAC", "DTS", "AC3", "YTS", "MX", "RARBG"
        };

        var parts = input.Split('.');

        // Remove file extension
        if (parts.Length > 0 && Regex.IsMatch(parts[^1], @"^[a-zA-Z0-9]+$"))
            parts = parts.Take(parts.Length - 1).ToArray();

        var titleParts = new List<string>();

        foreach (var part in parts)
        {
            // Stop if we hit a technical tag
            if (technicalTags.Any(tag => part.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0))
                break;

            // Stop if we hit a 4-digit year that's not at the beginning
            if (titleParts.Count > 0 && Regex.IsMatch(part, @"^\d{4}$"))
                break;

            // Clean the part but keep numbers for title parts like "2001"
            var cleanPart = Regex.Replace(part, @"[^A-Za-z0-9]", "");
            if (!string.IsNullOrWhiteSpace(cleanPart))
                titleParts.Add(cleanPart);
        }

        return string.Join(" ", titleParts);
    }

    public static int? ExtractYear(string input)
    {
        var parts = input.Split('.');

        foreach (var part in parts)
        {
            // Check if part is exactly 4 digits and within reasonable movie year range
            if (Regex.IsMatch(part, @"^\d{4}$"))
            {
                var year = int.Parse(part);
                // Reasonable year range for movies (1900-2030)
                if (year >= 1900 && year <= 2030)
                    return year;
            }
        }

        return null;
    }
}
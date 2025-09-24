namespace Features.Extensions;

public static class FileSizeExtensions
{
    public static string ToHumanReadableSize(this long? bytes)
    {
        if (bytes == null || bytes < 0)
            return "0 B";
        
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes.Value;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
}

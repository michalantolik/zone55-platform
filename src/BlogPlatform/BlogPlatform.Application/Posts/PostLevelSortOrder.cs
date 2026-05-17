namespace BlogPlatform.Application.Posts;

internal static class PostLevelSortOrder
{
    public static int FromLevel(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            return 50;
        }

        if (level.Contains("beginner", StringComparison.OrdinalIgnoreCase) ||
            level.Contains("basic", StringComparison.OrdinalIgnoreCase) ||
            level.Contains("fundamental", StringComparison.OrdinalIgnoreCase))
        {
            return 10;
        }

        if (level.Contains("intermediate", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (level.Contains("advanced", StringComparison.OrdinalIgnoreCase))
        {
            return 30;
        }

        return 50;
    }
}

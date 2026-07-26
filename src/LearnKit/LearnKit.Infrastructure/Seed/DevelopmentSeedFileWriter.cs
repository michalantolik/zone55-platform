namespace LearnKit.Infrastructure.Seed;

/// <summary>
/// Writes a generated seed to a fixed path inside a local solution checkout.
/// </summary>
public sealed class DevelopmentSeedFileWriter
{
    public async Task<string> WriteAsync(
        string relativePath,
        ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var solutionRoot = FindSolutionRoot()
            ?? throw new InvalidOperationException(
                "The Zone55 solution root could not be found. Start the API from inside a repository checkout.");

        var normalizedRelativePath = relativePath.Replace(
            '/',
            Path.DirectorySeparatorChar);
        var targetPath = Path.GetFullPath(
            Path.Combine(solutionRoot, normalizedRelativePath));
        var rootPrefix = solutionRoot.EndsWith(Path.DirectorySeparatorChar)
            ? solutionRoot
            : solutionRoot + Path.DirectorySeparatorChar;

        if (!targetPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The seed target must be located inside the solution root.");
        }

        var targetDirectory = Path.GetDirectoryName(targetPath)
            ?? throw new InvalidOperationException(
                "The seed target directory could not be resolved.");

        if (!Directory.Exists(targetDirectory))
        {
            throw new DirectoryNotFoundException(
                $"The seed target directory does not exist: {targetDirectory}");
        }

        if (!File.Exists(targetPath))
        {
            throw new FileNotFoundException(
                "The existing seed file could not be found and will not be created automatically.",
                targetPath);
        }

        var temporaryPath = Path.Combine(
            targetDirectory,
            $".{Path.GetFileName(targetPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await File.WriteAllBytesAsync(
                temporaryPath,
                content.ToArray(),
                cancellationToken);

            File.Move(temporaryPath, targetPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        return Path.GetRelativePath(solutionRoot, targetPath)
            .Replace(Path.DirectorySeparatorChar, '/');
    }

    private static string? FindSolutionRoot()
    {
        foreach (var startingPath in new[]
                 {
                     Directory.GetCurrentDirectory(),
                     AppContext.BaseDirectory
                 })
        {
            var directory = new DirectoryInfo(startingPath);

            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Zone55.slnx")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        return null;
    }
}

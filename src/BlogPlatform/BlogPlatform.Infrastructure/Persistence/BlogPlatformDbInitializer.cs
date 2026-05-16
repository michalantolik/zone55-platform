using BlogPlatform.Application.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlogPlatform.Infrastructure.Persistence;

public static class BlogPlatformDbInitializer
{
    public static async Task EnsureBlogPlatformSchemaAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<BlogPlatformDbContext>();

        var roadmapStore = scope.ServiceProvider
            .GetRequiredService<IDotnetRoadmapStore>();

        var configuration = scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILogger<BlogPlatformDbContext>>();

        logger.LogInformation(
            "Ensuring BlogPlatform roadmap schema and tables exist.");

        await EnsureSchemaAsync(dbContext, cancellationToken);

        await EnsureTablesAsync(dbContext, cancellationToken);

        var syncDefaultRoadmapOnStartup = configuration.GetValue<bool>(
            "Roadmap:SyncDefaultRoadmapOnStartup");

        var hasZones = await dbContext.RoadmapZones
            .AsNoTracking()
            .AnyAsync(cancellationToken);

        if (syncDefaultRoadmapOnStartup)
        {
            logger.LogInformation(
                "Syncing default roadmap data into SQL storage.");

            await roadmapStore.SaveAsync(
                DotnetRoadmapDefaults.Create(),
                cancellationToken);

            return;
        }

        if (!hasZones)
        {
            logger.LogInformation(
                "Seeding default roadmap data into empty SQL storage.");

            await roadmapStore.SaveAsync(
                DotnetRoadmapDefaults.Create(),
                cancellationToken);
        }
    }

    private static async Task EnsureSchemaAsync(
        BlogPlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF NOT EXISTS (
                SELECT 1
                FROM sys.schemas
                WHERE name = 'blogPlatform'
            )
            BEGIN
                EXEC('CREATE SCHEMA [blogPlatform]')
            END
            """,
            cancellationToken);
    }

    private static async Task EnsureTablesAsync(
        BlogPlatformDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID('[blogPlatform].[BlogZone]', 'U') IS NULL
            BEGIN
                CREATE TABLE [blogPlatform].[BlogZone]
                (
                    [RoadmapZoneId] INT IDENTITY(1,1) NOT NULL,
                    [Key] NVARCHAR(100) NOT NULL,
                    [Name] NVARCHAR(200) NOT NULL,
                    [Order] INT NOT NULL,

                    CONSTRAINT [PK_BlogZone]
                        PRIMARY KEY ([RoadmapZoneId]),

                    CONSTRAINT [UX_BlogZone_Key]
                        UNIQUE ([Key])
                )
            END
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID('[blogPlatform].[BlogStep]', 'U') IS NULL
            BEGIN
                CREATE TABLE [blogPlatform].[BlogStep]
                (
                    [RoadmapStepId] INT IDENTITY(1,1) NOT NULL,
                    [RoadmapZoneId] INT NOT NULL,
                    [Key] NVARCHAR(100) NOT NULL,
                    [Name] NVARCHAR(200) NOT NULL,
                    [Order] INT NOT NULL,

                    CONSTRAINT [PK_BlogStep]
                        PRIMARY KEY ([RoadmapStepId]),

                    CONSTRAINT [FK_BlogStep_BlogZone]
                        FOREIGN KEY ([RoadmapZoneId])
                        REFERENCES [blogPlatform].[BlogZone]([RoadmapZoneId])
                        ON DELETE CASCADE,

                    CONSTRAINT [UX_BlogStep_Key]
                        UNIQUE ([Key])
                )
            END
            """,
            cancellationToken);
    }
}

namespace BlogPlatform.Application.Roadmap;

public sealed record RoadmapOperationResult(
    bool Success,
    RoadmapOperationError Error,
    string Message)
{
    public static RoadmapOperationResult Ok(string message)
    {
        return new RoadmapOperationResult(
            true,
            RoadmapOperationError.None,
            message);
    }

    public static RoadmapOperationResult Fail(
        RoadmapOperationError error,
        string message)
    {
        return new RoadmapOperationResult(
            false,
            error,
            message);
    }
}

public enum RoadmapOperationError
{
    None = 0,
    ZoneNameRequired = 1,
    ZoneKeyAlreadyExists = 2,
    ZoneNotFound = 3,
    ZoneHasAssignedArticles = 4,
    StepNameRequired = 5,
    StepKeyAlreadyExists = 6,
    StepNotFound = 7,
    StepHasAssignedArticles = 8,
    ZoneRequired = 9,
    StepRequired = 10,
    StepDoesNotBelongToZone = 11
}

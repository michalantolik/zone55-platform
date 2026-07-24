namespace LearnKit.Application.Roadmaps.Admin;

/// <summary>
/// Raised when a learning structure key is already used.
/// </summary>
public sealed class LearningStructureKeyConflictException : Exception
{
    public LearningStructureKeyConflictException(string itemType, string key)
        : base($"The {itemType} key '{key}' is already in use.")
    {
        ItemType = itemType;
        Key = key;
    }

    public string ItemType { get; }
    public string Key { get; }
}

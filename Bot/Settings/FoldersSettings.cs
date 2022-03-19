namespace Bot.Settings;

public record FoldersSettings
{
    public const string SectionName = "Folders";

    public string InputFolder { get; init; }

    public string OutputFolder { get; init; }
}

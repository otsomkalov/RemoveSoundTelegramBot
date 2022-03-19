namespace Bot.Settings;

public record WorkersSettings
{
    public const string SectionName = "Workers";

    public string DownloaderQueueUrl { get; init; }

    public string ConverterQueueUrl { get; init; }

    public string UploaderQueueUrl { get; init; }

    public TimeSpan Delay { get; init; }
}
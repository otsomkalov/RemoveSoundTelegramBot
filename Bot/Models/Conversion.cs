namespace Bot.Models;

public record Conversion
{
    public int Id { get; init; }

    public long ChatId { get; init; }

    public int SentMessageId { get; init; }

    public string OutputFilePath { get; set; }
}

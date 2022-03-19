namespace Bot.Messages;

public record ConverterMessage
{
    public int Id { get; init; }

    public string InputFilePath { get; init; }

    public string Arguments { get; init; }

    public string OutputFilePath { get; init; }
}
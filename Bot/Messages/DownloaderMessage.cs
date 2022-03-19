using Telegram.Bot.Types;

namespace Bot.Messages;

public record DownloaderMessage
{
    public Message SentMessage { get; init; }
}
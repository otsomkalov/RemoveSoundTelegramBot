namespace Bot.Settings;

public record TelegramSettings
{
    public const string SectionName = "Telegram";

    public string Token { get; init; }
}

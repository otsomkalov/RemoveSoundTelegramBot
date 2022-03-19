using Bot.Constants;
using Telegram.Bot;
using Message = Telegram.Bot.Types.Message;

namespace Bot.Services;

public class MessageService
{
    private readonly ITelegramBotClient _bot;
    private readonly SQSService _sqsService;

    public MessageService(ITelegramBotClient bot, SQSService sqsService)
    {
        _bot = bot;
        _sqsService = sqsService;
    }

    public async Task HandleAsync(Message message)
    {
        if (message.From.IsBot)
        {
            return;
        }

        if (message.Text?.StartsWith("/start") == true)
        {
            await _bot.SendTextMessageAsync(new(message.From.Id), "Welcome! Send me a video file as video or document to remove sound from it.");

            return;
        }

        if (message.Video != null)
        {
            await SendDownloaderMessage(message, FileTypes.Video);

            return;
        }

        if (message.Document != null)
        {
            await ProcessDocumentMessage(message);
        }
    }

    private async Task ProcessDocumentMessage(Message receivedMessage)
    {
        if (string.IsNullOrEmpty(receivedMessage.Document.MimeType))
        {
            await _bot.SendTextMessageAsync(new(receivedMessage.From.Id),
                "Cannot determine type of input file",
                replyToMessageId: receivedMessage.MessageId);

            return;
        }

        if (!receivedMessage.Document.MimeType.StartsWith("video/", StringComparison.InvariantCultureIgnoreCase))
        {
            await _bot.SendTextMessageAsync(new(receivedMessage.From.Id),
                "Your file is not a video",
                replyToMessageId: receivedMessage.MessageId);

            return;
        }

        await SendDownloaderMessage(receivedMessage, FileTypes.Document);
    }

    private async Task SendDownloaderMessage(Message receivedMessage, string fileType)
    {
        var sentMessage =
            await _bot.SendTextMessageAsync(new(receivedMessage.From.Id), "File is waiting to be downloaded 🕒", replyToMessageId: receivedMessage.MessageId);

        await _sqsService.SendDownloaderMessage(sentMessage, fileType);
    }
}
using Amazon.SQS;
using Amazon.SQS.Model;
using Bot.Constants;
using Bot.Data;
using Bot.Messages;
using Bot.Models;
using Bot.Services;
using Bot.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace Bot.Workers;

public class DownloaderWorker : BackgroundService
{
    private readonly ILogger<DownloaderWorker> _logger;
    private readonly IAmazonSQS _sqsClient;
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly FoldersSettings _foldersSettings;
    private readonly WorkersSettings _workersSettings;
    private readonly SQSService _sqsService;

    public DownloaderWorker(ILogger<DownloaderWorker> logger, IAmazonSQS sqsClient, ITelegramBotClient bot,
        IServiceScopeFactory serviceScopeFactory, IOptions<FoldersSettings> foldersSettings, IOptions<WorkersSettings> workersSettings, SQSService sqsService)
    {
        _logger = logger;
        _sqsClient = sqsClient;
        _bot = bot;
        _serviceScopeFactory = serviceScopeFactory;
        _sqsService = sqsService;
        _workersSettings = workersSettings.Value;
        _foldersSettings = foldersSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();

        var serviceProvider = serviceScope.ServiceProvider;
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(dbContext, stoppingToken);

                await Task.Delay(_workersSettings.Delay, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during DownloaderWorker execution:");
            }
        }
    }

    private async Task RunAsync(AppDbContext appDbContext, CancellationToken cancellationToken)
    {
        var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = _workersSettings.DownloaderQueueUrl,
            WaitTimeSeconds = 20,
            MaxNumberOfMessages = 1,
            MessageAttributeNames = new()
            {
                MessageAttributes.Type
            }
        }, cancellationToken);

        var queueMessage = response.Messages.FirstOrDefault();

        if (queueMessage == null)
        {
            return;
        }

        var downloaderMessage = JsonSerializer.Deserialize<DownloaderMessage>(queueMessage.Body)!;

        await _bot.EditMessageTextAsync(new(downloaderMessage.SentMessage.Chat.Id),
            downloaderMessage.SentMessage.MessageId,
            "Downloading file 🚀",
            cancellationToken: cancellationToken);

        FileBase fileInfo = queueMessage.MessageAttributes[MessageAttributes.Type].StringValue switch
        {
            FileTypes.Document => downloaderMessage.SentMessage.ReplyToMessage!.Document!,
            FileTypes.Video => downloaderMessage.SentMessage.ReplyToMessage!.Video!
        };

        var fileName = queueMessage.MessageAttributes[MessageAttributes.Type].StringValue switch
        {
            FileTypes.Document => downloaderMessage.SentMessage.ReplyToMessage.Document!.FileName!,
            FileTypes.Video => downloaderMessage.SentMessage.ReplyToMessage.Video!.FileName!
        };

        var inputFilePath = Path.Combine(_foldersSettings.InputFolder, fileName);

        await using (var inputFileStream = File.Create(inputFilePath))
        {
            await _bot.GetInfoAndDownloadFileAsync(fileInfo.FileId, inputFileStream, cancellationToken);
        }

        _logger.LogInformation("Downloaded file {FileName}", fileName);

        var outputFilePath = Path.Combine(_foldersSettings.OutputFolder, fileName);

        var conversion = await SaveConversionToDatabase(downloaderMessage, outputFilePath, appDbContext, cancellationToken);

        await _sqsService.SendConverterMessage(conversion.Id, inputFilePath, outputFilePath, cancellationToken);

        await _bot.EditMessageTextAsync(new(downloaderMessage.SentMessage.Chat.Id),
            downloaderMessage.SentMessage.MessageId,
            "Conversion in progress 🚀",
            cancellationToken: cancellationToken);

        await _sqsClient.DeleteMessageAsync(_workersSettings.DownloaderQueueUrl, queueMessage.ReceiptHandle, cancellationToken);
    }

    private static async Task<Conversion> SaveConversionToDatabase(DownloaderMessage downloaderMessage, string outputFilePath,
        AppDbContext appDbContext, CancellationToken cancellationToken)
    {
        var conversion = new Conversion
        {
            ChatId = downloaderMessage.SentMessage.Chat.Id,
            SentMessageId = downloaderMessage.SentMessage.MessageId,
            OutputFilePath = outputFilePath
        };

        await appDbContext.Conversions.AddAsync(conversion, cancellationToken);
        await appDbContext.SaveChangesAsync(cancellationToken);

        return conversion;
    }
}

using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Bot.Data;
using Bot.Messages;
using Bot.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Workers;

public class UploaderWorker : BackgroundService
{
    private readonly ILogger<UploaderWorker> _logger;
    private readonly IAmazonSQS _sqsClient;
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly WorkersSettings _workersSettings;

    public UploaderWorker(ILogger<UploaderWorker> logger, IAmazonSQS sqsClient, ITelegramBotClient bot,
        IServiceScopeFactory serviceScopeFactory, IOptions<WorkersSettings> workersSettings)
    {
        _logger = logger;
        _sqsClient = sqsClient;
        _bot = bot;
        _serviceScopeFactory = serviceScopeFactory;
        _workersSettings = workersSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();

        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(dbContext, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during UploaderWorker execution:");
            }

            await Task.Delay(_workersSettings.Delay, stoppingToken);
        }
    }

    private async Task RunAsync(AppDbContext appDbContext, CancellationToken cancellationToken)
    {
        var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = _workersSettings.UploaderQueueUrl,
            WaitTimeSeconds = 20,
            MaxNumberOfMessages = 1
        }, cancellationToken);

        var queueMessage = response.Messages.FirstOrDefault();

        if (queueMessage == null)
        {
            return;
        }

        var uploaderMessage = JsonSerializer.Deserialize<UploaderMessage>(queueMessage.Body);

        var conversion = await appDbContext.Conversions
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == uploaderMessage.Id, cancellationToken);

        await _bot.EditMessageTextAsync(new(conversion.ChatId), conversion.SentMessageId, "Your file is uploading 🚀",
            cancellationToken: cancellationToken);

        var fileInfo = new FileInfo(conversion.OutputFilePath);

        await using (var outputFileStream = fileInfo.OpenRead())
        {
            await _bot.SendDocumentAsync(new(conversion.ChatId), new InputMedia(outputFileStream, fileInfo.Name),
                cancellationToken: cancellationToken);
        }

        await _bot.DeleteMessageAsync(new(conversion.ChatId), conversion.SentMessageId, cancellationToken);

        await _sqsClient.DeleteMessageAsync(_workersSettings.UploaderQueueUrl, queueMessage.ReceiptHandle, cancellationToken);
    }
}
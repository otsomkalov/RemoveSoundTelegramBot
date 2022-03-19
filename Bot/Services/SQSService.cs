using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.SQS;
using Bot.Constants;
using Bot.Messages;
using Bot.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Bot.Services;

public class SQSService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IAmazonSQS _sqsClient;
    private readonly WorkersSettings _workersSettings;

    public SQSService(IAmazonSQS sqsClient, IOptions<WorkersSettings> workersSettings)
    {
        _sqsClient = sqsClient;
        _workersSettings = workersSettings.Value;
    }

    public async Task SendDownloaderMessage(Message sentMessage, string fileType)
    {
        var downloaderMessage = new DownloaderMessage
        {
            SentMessage = sentMessage
        };

        await _sqsClient.SendMessageAsync(new()
        {
            QueueUrl = _workersSettings.DownloaderQueueUrl,
            MessageBody = JsonSerializer.Serialize(downloaderMessage, JsonSerializerOptions),
            MessageAttributes = new()
            {
                {
                    MessageAttributes.Type, new()
                    {
                        DataType = "String",
                        StringValue = fileType
                    }
                }
            }
        });
    }

    public async Task SendConverterMessage(int conversionId, string inputFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        var converterMessage = new ConverterMessage
        {
            Id = conversionId,
            InputFilePath = inputFilePath,
            Arguments = "-c copy -an",
            OutputFilePath = outputFilePath
        };

        await _sqsClient.SendMessageAsync(new()
        {
            QueueUrl = _workersSettings.ConverterQueueUrl,
            MessageBody = JsonSerializer.Serialize(converterMessage, JsonSerializerOptions)
        }, cancellationToken);
    }
}
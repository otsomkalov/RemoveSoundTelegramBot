using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Bot.Data;
using Bot.Services;
using Bot.Settings;
using Bot.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Default")));

builder.Services
    .Configure<TelegramSettings>(configuration.GetSection(TelegramSettings.SectionName))
    .Configure<FoldersSettings>(configuration.GetSection(FoldersSettings.SectionName))
    .Configure<WorkersSettings>(configuration.GetSection(WorkersSettings.SectionName));

builder.Services
    .AddSingleton<MessageService>()
    .AddSingleton<SQSService>();

builder.Services
    .AddHostedService<DownloaderWorker>()
    .AddHostedService<UploaderWorker>();

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<TelegramSettings>>().Value;

    return new TelegramBotClient(settings.Token);
});

builder.Services
    .AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(new EnvironmentVariablesAWSCredentials(), RegionEndpoint.EUCentral1));

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllers()
    .AddNewtonsoftJson();

var app = builder.Build();

app.MapControllers();

app.Run();

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
var services = builder.Services;

// Add services to the container.

services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Default")));

services
    .Configure<TelegramSettings>(configuration.GetSection(TelegramSettings.SectionName))
    .Configure<FoldersSettings>(configuration.GetSection(FoldersSettings.SectionName))
    .Configure<WorkersSettings>(configuration.GetSection(WorkersSettings.SectionName));

services
    .AddSingleton<MessageService>()
    .AddSingleton<SQSService>();

services
    .AddHostedService<DownloaderWorker>()
    .AddHostedService<UploaderWorker>();

services.AddSingleton<ITelegramBotClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<TelegramSettings>>().Value;

    return new TelegramBotClient(settings.Token);
});

services
    .AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(new EnvironmentVariablesAWSCredentials(), RegionEndpoint.EUCentral1));

services.AddApplicationInsightsTelemetry();

services.AddControllers()
    .AddNewtonsoftJson();

var app = builder.Build();

app.MapControllers();

app.Run();

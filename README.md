# Remove Sound Telegram Bot

Telegram Bot used to remove sound from video

## Getting Started
### Prerequisites

- [.NET 6](https://dotnet.microsoft.com/download) or higher

### Installing

**Telegram:**
1. Contact to [@BotFather](https://t.me/BotFather) in Telegram
2. Create new bot
3. Copy bot token

**Project:**
1. Clone project
2. Update **appsettings.json**
3. Set **AWS_ACCESS_KEY_ID** and **AWS_SECRET_ACCESS_KEY** environment variables
4. Clone and configure [FFMpegSQSWorker](https://github.com/otsomkalov/FFMpegSQSWorker)
5. `dotnet run`

Alternatively you can use

## Usage

You can try this bot in [Telegram](https://t.me/WebMToMP4Bot)


## Built With

* [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) - .NET Client for Telegram Bot API
* [Xabe.FFmpeg](https://github.com/tomaszzmuda/Xabe.FFmpeg) - .NET Standard wrapper for FFmpeg
* [aws-sdk-net](https://github.com/aws/aws-sdk-net) - The official AWS SDK for .NET

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
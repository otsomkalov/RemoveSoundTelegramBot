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

Alternatively you can use `docker-compose` file

## Usage

You can try this bot in [Telegram](https://t.me/rmsndbot)


## Built With

* [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) - .NET Client for Telegram Bot API
* [aws-sdk-net](https://github.com/aws/aws-sdk-net) - The official AWS SDK for .NET

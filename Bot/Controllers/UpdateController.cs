﻿using Bot.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.Controllers;

[ApiController]
[Route("/update")]
public class UpdateController : ControllerBase
{
    private readonly MessageService _messageService;
    private readonly ILogger<UpdateController> _logger;

    public UpdateController(MessageService messageService, ILogger<UpdateController> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }

    [HttpPost]
    public async Task HandleUpdate(Update update)
    {
        var handleUpdateTask = update.Type switch
        {
            UpdateType.Message => _messageService.HandleAsync(update.Message),
            _ => Task.CompletedTask
        };

        try
        {
            await handleUpdateTask;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during processing update:");
        }
    }
}
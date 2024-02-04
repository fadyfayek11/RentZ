﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RentZ.Application.Services.Messages;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RentZ.Application.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessagesService _messagesService;

    public ChatHub(IMessagesService messagesService)
    {
        _messagesService = messagesService;
    }
    public override async Task OnConnectedAsync()
    {
        var id = Context.User?.Identities.ElementAt(0).Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;

        await Clients.Caller.SendAsync("Connected", $"{id} has joined");
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var id = Context.User?.Identities.ElementAt(0).Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;

        await _messagesService.SaveMessages(id);
        await Clients.Caller.SendAsync("DisConnected", $"{id} has joined");
    }
}
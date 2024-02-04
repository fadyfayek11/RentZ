using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RentZ.Application.Services.Messages;
using RentZ.Domain.Entities;
using RentZ.DTO.Messages;

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

    public async Task Send(int pageIndex, int pageSize,int conversationId, string receiverId, string message)
    {
        var senderId = Context.User.FindFirstValue("UserId");


        conversationId = conversationId == 0 ? await _messagesService.StartConversation(senderId,receiverId) : conversationId;
       
        await _messagesService.SetTempMessages(new MessageDto
        {
            SendAt = DateTime.Now,
            ConversationId = conversationId,
            Content = message,
        }, senderId!, receiverId);

        await Clients.Users(senderId!, receiverId.ToLower()).SendAsync("Send", await _messagesService.GetTempMessages(pageIndex, pageSize, senderId, conversationId));
    }
}
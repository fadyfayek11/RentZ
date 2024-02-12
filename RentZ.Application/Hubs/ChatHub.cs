using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using RentZ.Application.Services.Messages;
using RentZ.Domain.Entities;
using RentZ.DTO.Messages;

namespace RentZ.Application.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessagesService _messagesService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChatHub(IMessagesService messagesService, IHttpContextAccessor httpContextAccessor)
    {
        _messagesService = messagesService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task OnConnectedAsync()
    {
        var id = Context.User?.Identities.ElementAt(0).Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        
        var chatId = int.TryParse(_httpContextAccessor.HttpContext.Items["ChatId"].ToString(), out var chat) ? chat : 0;
        var joined = await _messagesService.JoinConversation(chatId, id);

        await Clients.Caller.SendAsync("Connected", $"{id} has joined : {joined}");
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var id = Context.User?.Identities.ElementAt(0).Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;

        var chatId = int.TryParse(_httpContextAccessor.HttpContext.Items["ChatId"].ToString(), out var chat) ? chat : 0;
       
        var isSavedMessages = await _messagesService.SaveMessages(chatId);
        var isUserLeft = await _messagesService.LeftConversation(chatId, id);

        await Clients.Caller.SendAsync("DisConnected", $"{id} has left and messages saved: {isSavedMessages}");
    }

    public async Task Send(int pageIndex, int pageSize, int propId, int conversationId, string receiverId, string message)
    {
        var senderId = Context.User.FindFirstValue("UserId");


        conversationId = conversationId == 0 ? await _messagesService.StartConversation(propId, senderId, receiverId) : conversationId;
       
        await _messagesService.SetTempMessages(new MessageDto
        {
            SendAt = DateTime.Now,
            ConversationId = conversationId,
            Content = message,
            SenderId = senderId,
            ReceiverId = receiverId
        }, conversationId, senderId!, receiverId);

        await Clients.Users(senderId!, receiverId.ToLower()).SendAsync("Send",  _messagesService.GetTempMessages(pageIndex, pageSize, senderId, conversationId));
    }

    public async Task Save()
    {
        var id = Context.User?.Identities.ElementAt(0).Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        var chatId = int.TryParse(_httpContextAccessor.HttpContext.Items["ChatId"].ToString(), out var chat) ? chat : 0;

        var isSaved = await _messagesService.SaveMessages(chatId);
        await Clients.Users(id!).SendAsync("Save", isSaved);
    }
    
    public async Task ChatHistory(int pageIndex, int pageSize, int conversationId)
    {
        var id = Context.User?.Identities.ElementAt(0).Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;

        var history = await _messagesService.GetDbMessages(pageIndex, pageSize, id, conversationId);
        await Clients.Users(id!).SendAsync("History", history);
    }
}
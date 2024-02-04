using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RentZ.Application.Services.Notification;
using RentZ.Domain.Entities;
using RentZ.DTO.Enums;
using RentZ.DTO.Messages;
using RentZ.DTO.Notification;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.Messages;

public class MessagesService : IMessagesService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private readonly INotificationService _notificationService;
    public MessagesService(ApplicationDbContext context, IMemoryCache memoryCache, INotificationService notificationService)
    {
        _context = context;
        _memoryCache = memoryCache;
        _notificationService = notificationService;
    }

    public async Task SetTempMessages(MessageDto message, string uId, string receiverId)
    {
        if (!_memoryCache.TryGetValue(uId, out List<MessageDto>? cachedList))
        {
            cachedList = new List<MessageDto>();
        }

        cachedList?.Add(message);
        await _notificationService.AddNotification(new AddNotification
        {
            Type = NotificationTypes.Message,
            Title = "Message",
            Content = "",
            LinkId = message.ConversationId,
            ReceiverId = receiverId,
            SenderId = uId
        });
        _memoryCache.Set(uId, cachedList);
    }
    public async Task<List<MessageDto>?> GetDbMessages(int pageIndex, int pageSize, int conversationId)
    {
        var messages = await _context.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId);
        return messages?.Messages?.Select(x=> new MessageDto
            {
                Id = x.Id,
                ConversationId = x.ConversationId,
                SendAt = x.SentAt,
                Content = x.Content,
                SenderId = x.Conversation.SenderId.ToString(),
                SenderName = x.Conversation.Sender.User.DisplayName,
                ReceiverId = x.Conversation.ReceiverId.ToString(),
                ReceiverName = x.Conversation.Receiver.User.DisplayName,
            })
            .Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderByDescending(x => x.SendAt).ToList();
    }
    public async Task<List<MessageDto>?> GetTempMessages(int pageIndex, int pageSize, string uId, int conversationId)
    {
        if (!_memoryCache.TryGetValue(uId, out List<MessageDto>? cachedList)) return await GetDbMessages(pageIndex, pageSize, conversationId);
        if (cachedList != null) return cachedList.Concat(await GetDbMessages(pageIndex, pageSize,conversationId) ?? new List<MessageDto>()).ToList();

        return new List<MessageDto>();
    }
    public async Task<bool> SaveMessages(string uId)
    {
        if (_memoryCache.TryGetValue(uId, out List<Message>? cachedList))
        {
            if (cachedList == null) return true;

            await _context.Messages.AddRangeAsync(cachedList);
            var conversation = await _context.Conversations.FirstOrDefaultAsync(x => x.Id == cachedList[0].ConversationId);
            if (conversation != null)
            {
                conversation.CreationDate = DateTime.Now;
                _context.Conversations.Update(conversation);
            }
            await _context.SaveChangesAsync();

            cachedList.Clear();
            _memoryCache.Set(uId, cachedList);
        }
        return true;
    }
    public async Task<int> StartConversation(string senderId, string receiverId)
    {
        var conversationExist = await _context.Conversations.FirstOrDefaultAsync(x =>
            (x.SenderId.ToString() == senderId || x.SenderId.ToString() == receiverId) &&
            (x.ReceiverId.ToString() == senderId || x.ReceiverId.ToString() == receiverId));

        if (conversationExist is not null)
        {
            return conversationExist.Id;
        }

        var conversation = await _context.Conversations.AddAsync(new Conversation
        {
            SenderId = Guid.Parse(senderId),
            ReceiverId = Guid.Parse(receiverId),
        });

        await _notificationService.AddNotification(new AddNotification
        {
            Type = NotificationTypes.Message,
            Title = "Message",
            Content = "",
            LinkId = conversation.Entity.Id,
            ReceiverId = receiverId,
            SenderId = senderId
        });
        await _context.SaveChangesAsync();

        return conversation.Entity.Id;
    }
}
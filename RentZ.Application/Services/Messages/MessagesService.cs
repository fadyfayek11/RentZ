using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RentZ.Application.Services.Notification;
using RentZ.Domain.Entities;
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

    public void SetTempMessages(Message message, string uId)
    {
        if (!_memoryCache.TryGetValue(uId, out List<Message>? cachedList))
        {
            cachedList = new List<Message>();
        }

        cachedList?.Add(message);

        _memoryCache.Set(uId, cachedList);
    }
    public async Task<List<Message>?> GetDbMessages(int pageIndex, int pageSize, int conversationId)
    {
        var messages = await _context.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId);
        return messages?.Messages.Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderByDescending(x => x.SentAt).ToList();
    }
    public async Task<List<Message>?> GetTempMessages(int pageIndex, int pageSize, string uId, int conversationId)
    {
        if (!_memoryCache.TryGetValue(uId, out List<Message>? cachedList)) return await GetDbMessages(pageIndex, pageSize, conversationId);
        if (cachedList != null) return cachedList.Concat(await GetDbMessages(pageIndex, pageSize,conversationId) ?? new List<Message>()).ToList();

        return new List<Message>();
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
        var conversation = await _context.Conversations.AddAsync(new Conversation
        {
            SenderId = Guid.Parse(senderId),
            ReceiverId = Guid.Parse(receiverId),
        });
        await _context.SaveChangesAsync();

        return conversation.Entity.Id;
    }
}
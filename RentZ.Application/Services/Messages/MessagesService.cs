using Microsoft.Extensions.Caching.Memory;
using RentZ.Domain.Entities;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.Messages;

public class MessagesService : IMessagesService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    public MessagesService(ApplicationDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }
    public void SetMessage(Message message, string uId)
    {
        if (!_memoryCache.TryGetValue(uId, out List<Message>? cachedList))
        {
            cachedList = new List<Message>();
        }

        cachedList?.Add(message);

        _memoryCache.Set(uId, cachedList);
    }
    public async Task<bool> SaveMessages(string uId)
    {
        if (_memoryCache.TryGetValue(uId, out List<Message>? cachedList))
        {
            if (cachedList != null) await _context.Messages.AddRangeAsync(cachedList);
            await _context.SaveChangesAsync();

            cachedList?.Clear();
            _memoryCache.Set(uId, cachedList);
        }
        return true;
    }
}
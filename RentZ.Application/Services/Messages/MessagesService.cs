using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RentZ.Application.Services.Notification;
using RentZ.Domain.Entities;
using RentZ.DTO.Enums;
using RentZ.DTO.Messages;
using RentZ.DTO.Notification;
using RentZ.DTO.Property;
using RentZ.DTO.Response;
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

        message.SenderName = await _context.Users.Where(x => x.Id.ToString() == uId).Select(x=>x.DisplayName).FirstOrDefaultAsync() ?? "";
        message.ReceiverName = await _context.Users.Where(x => x.Id.ToString() == receiverId).Select(x=>x.DisplayName).FirstOrDefaultAsync() ?? "";
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
    public async Task<PagedResult<MessageDto>?> GetDbMessages(int pageIndex, int pageSize, int conversationId)
    {
        var messages = await  _context.Conversations.Where(y => y.Id == conversationId)
            .Select(z=>z.Messages
                .Select(x => new MessageDto
                {
                    Id = x.Id,
                    ConversationId = x.ConversationId,
                    SendAt = x.SentAt,
                    Content = x.Content,
                    SenderId = x.Conversation.SenderId.ToString(),
                    SenderName = x.Conversation.Sender.User.DisplayName,
                    ReceiverId = x.Conversation.ReceiverId.ToString(),
                    ReceiverName = x.Conversation.Receiver.User.DisplayName,
                }).Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderBy(x => x.SendAt).ToList()
            ).FirstOrDefaultAsync();
       
        var totalCount = await _context.Conversations.Where(y => y.Id == conversationId)
            .Select(z => z.Messages.Count).FirstOrDefaultAsync();

        return new PagedResult<MessageDto>() { Items = messages, TotalCount =  totalCount };
    }
    public async Task<PagedResult<MessageDto>?> GetTempMessages(int pageIndex, int pageSize, string uId, int conversationId)
    {
        if (!_memoryCache.TryGetValue(uId, out List<MessageDto>? cachedList)) return await GetDbMessages(pageIndex, pageSize, conversationId);
        if (cachedList != null)
        {
            var dbMessages = await GetDbMessages(pageIndex, pageSize, conversationId);
            var totalCount = cachedList.Count() + dbMessages?.TotalCount;
            var totalMessages = cachedList.Concat(dbMessages?.Items ?? new List<MessageDto>());

            return new PagedResult<MessageDto>() { Items = totalMessages.ToList(), TotalCount = totalCount ?? 0 };
        }

        return new PagedResult<MessageDto>();
    }
    public async Task<bool> SaveMessages(string uId)
    {
        if (_memoryCache.TryGetValue(uId, out List<MessageDto>? cachedList))
        {
            if (cachedList == null) return true;
            
            var messagesEntity = cachedList.Select(x => new Message
            {
                ConversationId = x.ConversationId,
                Content = x.Content,
                SentAt = DateTime.Now,
            }).ToList();

            await _context.Messages.AddRangeAsync(messagesEntity);

            var conversation = await _context.Conversations.FirstOrDefaultAsync(x => x.Id == cachedList[0].ConversationId);
            if (conversation != null)
            {
                conversation.CreationDate = DateTime.Now;
                _context.Conversations.Update(conversation);
            }
            var isSaved =  await _context.SaveChangesAsync() > 0;

            cachedList.Clear();
            _memoryCache.Set(uId, cachedList);
            return isSaved;
        }
        return false;
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

    public async Task<BaseResponse<PagedResult<ConversationDto?>>> Conversations(Pagination pagination, string senderId, HttpContext context)
    {
        var conversations =  _context.Conversations.Where(x => 
            (x.SenderId == Guid.Parse(senderId) || x.ReceiverId == Guid.Parse(senderId)) && !x.IsRead);
        
        var conversationsCount = conversations.Count();
        var response = await conversations
            .Select(x=> new ConversationDto
            {
                Id = x.Id,
                SendAt = x.CreationDate,
                SenderId = x.SenderId.ToString(),
                SenderName = x.Sender.User.DisplayName,
                SenderImageUrl = GetProfileImageUrl(x.SenderId.ToString(), context),
                ReceiverId = x.ReceiverId.ToString(),
                ReceiverName = GetProfileImageUrl(x.ReceiverId.ToString(), context),
                ReceiverImageUrl = x.Receiver.User.DisplayName,
                IsRead = x.IsRead
            })
            .Skip((pagination.PageIndex - 1) * pagination.PageSize)
            .Take(pagination.PageSize).OrderByDescending(x => x.SendAt).ToListAsync();

        return new BaseResponse<PagedResult<ConversationDto?>> { Code = ErrorCode.Success, Message = "Get user Conversation done successfully", Data = new PagedResult<ConversationDto?>() { Items = response, TotalCount = conversationsCount } };
    }

    public async Task<BaseResponse<bool?>> ReadConversation(int conversationId, string uId)
    {
        var conversation = await _context.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId && 
            (x.SenderId.ToString() == uId || x.ReceiverId.ToString() == uId));
        if (conversation is null)
        {
            return new BaseResponse<bool?>()
                { Code = ErrorCode.BadRequest, Message = "Can't get the conversation", Data = false };
        }
        
        conversation.IsRead = true;
        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync();

        return new BaseResponse<bool?>()
            { Code = ErrorCode.Success, Message = "You read the conversation successfully", Data = true };
    }

    private static string GetProfileImageUrl(string uId, HttpContext context)
    {
        var request = context.Request;

        var scheme = request.Scheme;

        var host = request.Host.Value;

        return $"{scheme}://{host}/api/User/Profile?uId={uId}";
    }
}
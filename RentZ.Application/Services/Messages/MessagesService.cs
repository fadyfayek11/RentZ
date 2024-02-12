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
    public MessagesService(ApplicationDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }

    public async Task SetTempMessages(MessageDto message, int conversationId, string uId, string receiverId)
    {
        if (!_memoryCache.TryGetValue(conversationId, out List<MessageDto>? cachedList))
        {
            cachedList = new List<MessageDto>();
        }

        message.SenderName = await _context.Users.Where(x => x.Id.ToString() == uId).Select(x=>x.DisplayName).FirstOrDefaultAsync() ?? "";
        message.ReceiverName = await _context.Users.Where(x => x.Id.ToString() == receiverId).Select(x=>x.DisplayName).FirstOrDefaultAsync() ?? "";
        
        cachedList?.Add(message);
        _memoryCache.Set(conversationId, cachedList);

        var conversation = await _context.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId);
        
        if (conversation is { IsReceiverOnline: false, IsSenderOnline: true } or { IsReceiverOnline: true, IsSenderOnline: false })
        {
           await SaveMessages(conversationId);
        }
    }
   
    public async Task<PagedResult<MessageDto>?> GetDbMessages(int pageIndex, int pageSize, string uId, int conversationId)
    {
        var messages = await  _context.Conversations.Include(x=>x.Messages).Where(y => y.Id == conversationId)
            .Select(z=> z.Messages
                .Select(x => new MessageDto
                {
                    Id = x.Id,
                    ConversationId = x.ConversationId,
                    SendAt = x.SentAt,
                    Content = x.Content,
                    SenderId = x.SenderId.ToString(),
                    SenderName = x.Sender.User.DisplayName,
                    ReceiverId = x.ReceiverId.ToString(),
                    ReceiverName = x.Receiver.User.DisplayName,
                }).OrderByDescending(x => x.SendAt).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
            ).FirstOrDefaultAsync();
       
        var totalCount = await _context.Conversations.Where(y => y.Id == conversationId)
            .Select(z => z.Messages.Count).FirstOrDefaultAsync();
        
        if (!_memoryCache.TryGetValue(conversationId, out List<MessageDto>? cachedList))
        {
            cachedList = new List<MessageDto>();
        }

        cachedList?.AddRange(messages);
        cachedList = cachedList?.GroupBy(x=>x.Id).Select(x=>x.First()).ToList();

        _memoryCache.Set(conversationId, cachedList);

        return new PagedResult<MessageDto>() { Items = messages, TotalCount =  totalCount };
    }
   
    public PagedResult<MessageDto>? GetTempMessages(int pageIndex, int pageSize, string uId, int conversationId)
    {
        if (!_memoryCache.TryGetValue(conversationId, out List<MessageDto>? cachedList)) return new PagedResult<MessageDto>() ;
        
        if (cachedList is not null && cachedList.Any())
        {
            var totalCount = cachedList.Count();
            return new PagedResult<MessageDto>() { Items = cachedList.OrderByDescending(x=>x.SendAt).ToList(), TotalCount = totalCount };
        }

        return new PagedResult<MessageDto>();
    }
    
    public async Task<bool> SaveMessages(int conversationId)
    {
        if (_memoryCache.TryGetValue(conversationId, out List<MessageDto>? cachedList))
        {
            if (!cachedList.Any()) return true;
            
            var messagesEntity = cachedList.Where(x=>x.Id == 0).Select(x => new Message
            {
                ConversationId = x.ConversationId,
                Content = x.Content,
                SentAt = x.SendAt,
                SenderId = Guid.Parse(x.SenderId),
                ReceiverId = Guid.Parse(x.ReceiverId),
            }).ToList();

            await _context.Messages.AddRangeAsync(messagesEntity);

            var conversation = await _context.Conversations.FirstOrDefaultAsync(x => x.Id == cachedList[0].ConversationId);
            if (conversation != null)
            {
                conversation.CreationDate = DateTime.Now;
                
                if (conversation.IsReceiverOnline is false && conversation.IsSenderOnline && messagesEntity.Count() is not 0)
                {
                    conversation.IsReadByReceiver = false;
                    conversation.IsReadBySender = true;
                }

                if (conversation.IsSenderOnline is false && conversation.IsReceiverOnline && messagesEntity.Count() is not 0)
                {
                    conversation.IsReadBySender = false;
                    conversation.IsReadByReceiver = true;
                }

                _context.Conversations.Update(conversation);
            }
            var isSaved =  await _context.SaveChangesAsync() > 0;

            if (conversation is { IsReceiverOnline: false, IsSenderOnline: false })
            {
                cachedList.Clear();
                _memoryCache.Set(conversationId, cachedList);

                return isSaved;
            }

            cachedList.RemoveRange(cachedList.Count() - messagesEntity.Count(),messagesEntity.Count());
            cachedList.AddRange(messagesEntity.Select(x=> new MessageDto
            {
                Id = x.Id,
                ConversationId = x.ConversationId,
                SendAt = x.SentAt,
                Content = x.Content,
                SenderId = x.SenderId.ToString(),
                SenderName = _context.Users.Where(z => z.Id == x.SenderId).Select(c => c.DisplayName).FirstOrDefault() ?? "",
                ReceiverId = x.ReceiverId.ToString(),
                ReceiverName = _context.Users.Where(z => z.Id == x.ReceiverId).Select(c => c.DisplayName).FirstOrDefault() ?? "",
            }).ToList());

            _memoryCache.Set(conversationId, cachedList);
            return isSaved;
        }
        return false;
    }

    public async Task<int> StartConversation(int propId, string senderId, string receiverId)
    {
        var conversationId = await ConversationExist(senderId, receiverId);

        if (conversationId > 0)
        {
            return conversationId;
        }

        var conversation = await _context.Conversations.AddAsync(new Conversation
        {
            PropId = propId,
            SenderId = Guid.Parse(senderId),
            ReceiverId = Guid.Parse(receiverId),
            IsReadBySender = true,
            IsReadByReceiver = false
        });

        await _context.SaveChangesAsync();

        return conversation.Entity.Id;
    }

    public async Task<int> ConversationExist(string senderId, string receiverId)
    {
        var conversationExist = await _context.Conversations.FirstOrDefaultAsync(x =>
            (x.SenderId.ToString() == senderId || x.SenderId.ToString() == receiverId) &&
            (x.ReceiverId.ToString() == senderId || x.ReceiverId.ToString() == receiverId));
        return conversationExist?.Id ?? 0; 
    }

    public async Task<BaseResponse<PagedResult<ConversationDto?>>> Conversations(Pagination pagination, string uId, HttpContext context)
    {
        var conversations =  _context.Conversations.Where(x => 
            (x.SenderId == Guid.Parse(uId) || x.ReceiverId == Guid.Parse(uId)));
        
        var conversationsCount = conversations.Count();
        var response = await conversations
            .Select(x=> new ConversationDto
            {
                Id = x.Id,
                PropId = x.PropId,
                OwnerId =  _context.Properties.Where(y => y.Id == x.PropId).Select(z=>z.OwnerId).FirstOrDefault().ToString(),
                SendAt = x.CreationDate,
                SenderId = uId != x.SenderId.ToString() ? x.SenderId.ToString() : x.ReceiverId.ToString(),
                SenderName = uId != x.SenderId.ToString() ? x.Sender.User.DisplayName : x.Receiver.User.DisplayName,
                SenderImageUrl = uId != x.SenderId.ToString() ? GetProfileImageUrl(x.SenderId.ToString(), context) : GetProfileImageUrl(x.ReceiverId.ToString(), context),
                ReceiverId = uId != x.SenderId.ToString() ? x.ReceiverId.ToString() : x.SenderId.ToString(),
                ReceiverName = uId != x.SenderId.ToString() ? x.Receiver.User.DisplayName : x.Sender.User.DisplayName,
                ReceiverImageUrl = uId != x.SenderId.ToString() ? GetProfileImageUrl(x.ReceiverId.ToString(), context) : GetProfileImageUrl(x.SenderId.ToString(), context),
                IsReadBySender = uId != x.SenderId.ToString() ? x.IsReadBySender : x.IsReadByReceiver,
                IsSenderOnline = uId != x.SenderId.ToString() ? x.IsSenderOnline : x.IsReceiverOnline,
                IsReadByReceiver = uId != x.SenderId.ToString() ? x.IsReadByReceiver : x.IsReadBySender,
                IsReceiverOnline = uId != x.SenderId.ToString() ? x.IsReceiverOnline : x.IsSenderOnline
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
        var isSender = conversation.SenderId.ToString() == uId;
        
        if (isSender)
        {
            conversation.IsReadBySender = true;
        }
        else
        {
            conversation.IsReadByReceiver = true;
        }

        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync();

        return new BaseResponse<bool?>()
            { Code = ErrorCode.Success, Message = "You read the conversation successfully", Data = true };
    }

    public async Task<bool> JoinConversation(int conversationId, string uId)
    {
        var conversation = await _context.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId);
        if (conversation is null)
        {
            return false;
        }
        var isSender = conversation.SenderId.ToString() == uId;

        if (isSender)
        {
            conversation.IsSenderOnline = true;
        }
        else
        {
            conversation.IsReceiverOnline = true;
        }

        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> LeftConversation(int conversationId, string uId)
    {
        var conversation = await _context.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId);
        if (conversation is null)
        {
            return false;
        }
        var isSender = conversation.SenderId.ToString() == uId;

        if (isSender)
        {
            conversation.IsSenderOnline = false;
        }
        else
        {
            conversation.IsReceiverOnline = false;
        }

        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync();

        return true;
    }

    private static string GetProfileImageUrl(string uId, HttpContext context)
    {
        var request = context.Request;

        var scheme = request.Scheme;

        var host = request.Host.Value;

        return $"{scheme}://{host}/api/User/Profile?uId={uId}";
    }
}
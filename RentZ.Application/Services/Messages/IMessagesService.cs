using Microsoft.AspNetCore.Http;
using RentZ.Domain.Entities;
using RentZ.DTO.Messages;
using RentZ.DTO.Property;
using RentZ.DTO.Response;

namespace RentZ.Application.Services.Messages;

public interface IMessagesService
{
    Task SetTempMessages(MessageDto request, string uId, string receiverId);
    Task<PagedResult<MessageDto>?> GetDbMessages(int pageIndex, int pageSize, int conversationId);
    Task<PagedResult<MessageDto>?> GetTempMessages(int pageIndex, int pageSize, string uId, int conversationId);
    Task<bool> SaveMessages(string uId);
    Task<int> StartConversation(string senderId, string receiverId);
    Task<BaseResponse<PagedResult<ConversationDto?>>> Conversations(Pagination pagination, string uId, HttpContext context);
    Task<BaseResponse<bool?>> ReadConversation(int conversationId, string uId);
}
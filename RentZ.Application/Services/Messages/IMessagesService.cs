using Microsoft.AspNetCore.Http;
using RentZ.DTO.Messages;
using RentZ.DTO.Property;
using RentZ.DTO.Response;

namespace RentZ.Application.Services.Messages;

public interface IMessagesService
{
    Task SetTempMessages(MessageDto request, int chatId, string uId, string receiverId);
    Task<PagedResult<MessageDtoResponse>?> GetDbMessages(int pageIndex, int pageSize, string uId, int conversationId);
    PagedResult<MessageDto>? GetTempMessages(int pageIndex, int pageSize, string uId, int conversationId);
    Task<bool> SaveMessages(int conversationId);
    Task<int> StartConversation(int propId, string senderId, string receiverId);
    Task<int> ConversationExist(string senderId, string receiverId, int propId);
    Task<BaseResponse<PagedResult<ConversationDto?>>> Conversations(Pagination pagination, string uId, HttpContext context);
    Task<BaseResponse<bool?>> ReadConversation(int conversationId, string uId);
    Task<BaseResponse<bool?>> RemoveConversation(int conversationId, string uId);
    Task<bool> JoinConversation(int conversationId, string uId);
    Task<bool> LeftConversation(int conversationId, string uId);
    bool UserHasProp(string uId);
}
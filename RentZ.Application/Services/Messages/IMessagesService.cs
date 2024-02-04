using RentZ.Domain.Entities;
using RentZ.DTO.Messages;

namespace RentZ.Application.Services.Messages;

public interface IMessagesService
{
    Task SetTempMessages(MessageDto request, string uId, string receiverId);
    Task<List<MessageDto>?> GetDbMessages(int pageIndex, int pageSize, int conversationId);
    Task<List<MessageDto>?> GetTempMessages(int pageIndex, int pageSize, string uId, int conversationId);
    Task<bool> SaveMessages(string uId);
    Task<int> StartConversation(string senderId, string receiverId);

}
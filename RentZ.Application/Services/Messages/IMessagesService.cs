using RentZ.Domain.Entities;

namespace RentZ.Application.Services.Messages;

public interface IMessagesService
{
    Task SetTempMessages(Message request, string uId);
    Task<List<Message>?> GetDbMessages(int pageIndex, int pageSize, int conversationId);
    Task<List<Message>?> GetTempMessages(int pageIndex, int pageSize, string uId, int conversationId);
    Task<bool> SaveMessages(string uId);
    Task<int> StartConversation(string senderId, string receiverId);

}
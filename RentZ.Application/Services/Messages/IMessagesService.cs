using RentZ.Domain.Entities;

namespace RentZ.Application.Services.Messages;

public interface IMessagesService
{
    void SetMessage(Message request, string uId);
    Task<bool> SaveMessages(string uId);

}
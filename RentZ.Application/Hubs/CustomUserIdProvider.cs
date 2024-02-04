using Microsoft.AspNetCore.SignalR;

namespace RentZ.Application.Hubs;

public class CustomUserIdProvider : IUserIdProvider
{
    public virtual string? GetUserId(HubConnectionContext connection)
    {
        var id = connection.User?.Identities.ElementAt(0).Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        return id;
    }
}

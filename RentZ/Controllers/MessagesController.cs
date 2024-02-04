using Microsoft.AspNetCore.Mvc;
using RentZ.Application.Hubs;
using RentZ.DTO.Response;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using RentZ.Application.Services.Messages;
using RentZ.Domain.Entities;
using RentZ.DTO.Enums;
using RentZ.DTO.Messages;

namespace RentZ.API.Controllers;


[Route("api/[controller]")]
[ApiController]
public class MessagesController : Controller
{
    private readonly IHubContext<ChatHub> _context;
    private readonly IMessagesService _messagesService;
    public MessagesController(IHubContext<ChatHub> context, IMessagesService messagesService)
    {
        _context = context;
        _messagesService = messagesService;
    }

    [Authorize]
    [HttpPost(nameof(Send))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<List<Message>>))]
    public async Task<IActionResult> Send(int pageIndex, int pageSize, int conversationId, string receiverId, string message)
    {
        var senderId = HttpContext.User.FindFirstValue("UserId");


        conversationId = conversationId == 0 ? await _messagesService.StartConversation(senderId, receiverId) : conversationId;

        await _messagesService.SetTempMessages(new MessageDto
        {
            ConversationId = conversationId,
            Content = message,
        }, senderId!, receiverId);

        var listOfMessages = await _messagesService.GetTempMessages(pageIndex, pageSize, senderId, conversationId);
        
        await _context.Clients.Users(senderId!, receiverId.ToLower()).SendAsync("Send", listOfMessages);
        
        return new OkObjectResult(new BaseResponse<List<MessageDto>>()
            { Data = listOfMessages, Code = ErrorCode.Success }
        );

    }
}
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
using RentZ.DTO.Property;

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
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<Message>>))]
    public async Task<IActionResult> Send(int pageIndex, int pageSize, int conversationId, string receiverId, string message)
    {
        var senderId = HttpContext.User.FindFirstValue("UserId");


        conversationId = conversationId == 0 ? await _messagesService.StartConversation(senderId, receiverId) : conversationId;

        await _messagesService.SetTempMessages(new MessageDto
        {
            SendAt = DateTime.Now,
            ConversationId = conversationId,
            Content = message,
            SenderId = senderId!,
            ReceiverId = receiverId,
        }, senderId!, receiverId);

        var listOfMessages = await _messagesService.GetTempMessages(pageIndex, pageSize, senderId, conversationId);
        
        await _context.Clients.Users(senderId!, receiverId.ToLower()).SendAsync("Send", listOfMessages);
        
        return new OkObjectResult(new BaseResponse<PagedResult<MessageDto>>()
            { Data = listOfMessages, Code = ErrorCode.Success }
        );

    }
    
    [Authorize]
    [HttpPost(nameof(Conversations))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<ConversationDto>>))]
    public async Task<IActionResult> Conversations(Pagination pagination)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");

        var response = await _messagesService.Conversations(pagination, uId, HttpContext);

        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };

    }
}
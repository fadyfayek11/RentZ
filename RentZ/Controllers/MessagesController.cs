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


[Route("api/")]
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
    [HttpPost("Messages/Send", Name = "Send")]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<Message>>))]
    public async Task<IActionResult> Send(int pageIndex, int pageSize, int conversationId, int propId, string receiverId, string message)
    {
        var senderId = HttpContext.User.FindFirstValue("UserId");

        var existConversationId = await _messagesService.ConversationExist(senderId, receiverId, propId);
        if (existConversationId > 0)
        {
            return new BadRequestObjectResult(new { chatId = existConversationId});
        }

        conversationId = await _messagesService.StartConversation(propId, senderId, receiverId);

        await _messagesService.SetTempMessages(new MessageDto
        {
            SendAt = DateTime.UtcNow,
            ConversationId = conversationId,
            Content = message,
            SenderId = senderId!,
            ReceiverId = receiverId,
        }, conversationId, senderId!, receiverId);

        await _messagesService.SaveMessages(conversationId);
        var listOfMessages =  await _messagesService.GetDbMessages(pageIndex, pageSize, senderId, conversationId);
        
        await _context.Clients.Users(senderId!, receiverId.ToLower()).SendAsync("Send", listOfMessages);
        
        return new OkObjectResult(new BaseResponse<PagedResult<MessageDtoResponse>>()
            { Data = listOfMessages, Code = ErrorCode.Success }
        );

    }
    
    [Authorize]
    [HttpGet(nameof(Conversations))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<ConversationDto>>))]
    public async Task<IActionResult> Conversations([FromQuery]Pagination pagination)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");

        var response = await _messagesService.Conversations(pagination, uId, HttpContext);

        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };

    }
    
    [Authorize]
    [HttpPatch("Conversations/Read", Name = "Read")]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
    public async Task<IActionResult> ReadConversations(int conversationId)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");

        var response = await _messagesService.ReadConversation(conversationId, uId);

        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };

    }
    
    
    [Authorize]
    [HttpDelete("Conversations/Remove", Name = "Remove")]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
    public async Task<IActionResult> RemoveConversations(int conversationId)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");

        var response = await _messagesService.RemoveConversation(conversationId, uId);

        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };

    }

}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentZ.DTO.Enums;
using RentZ.DTO.Response;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using RentZ.Application.Services.Notification;
using RentZ.DTO.Property;
using RentZ.DTO.Feedback;
using RentZ.DTO.Notification;

namespace RentZ.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;
    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [Authorize(Roles = "Client")]
    [HttpGet(nameof(Count))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<int?>))]
    public async Task<IActionResult> Count()
    {
        var uId = HttpContext.User.FindFirstValue("UserId");
        var response = await _notificationService.NotificationCount(uId);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }
    
    [Authorize(Roles = "Client")]
    [HttpPatch(nameof(Read))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool?>))]
    public async Task<IActionResult> Read([FromQuery] int id)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");
        var response = await _notificationService.ReadNotification(id, uId);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }
    
    [Authorize(Roles = "Client")]
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<GetNotifications?>>))]
    public async Task<IActionResult> Notifications([FromQuery] Pagination pagination)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");
        var response = await _notificationService.NotificationsList(pagination, uId);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }
    
    [Authorize]
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<bool?>>))]
    public async Task<IActionResult> Notification(AddNotification request)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");
        request.SenderId = uId;

        var response = await _notificationService.AddNotification(request);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }
}
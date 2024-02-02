using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentZ.DTO.Enums;
using RentZ.DTO.Response;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using RentZ.Application.Services.Notification;

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
}
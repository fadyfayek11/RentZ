using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentZ.Application.Services.Feedback;
using RentZ.DTO.Enums;
using RentZ.DTO.Feedback;
using RentZ.DTO.Property;
using RentZ.DTO.Response;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace RentZ.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeedbackController : Controller
{
    private readonly IFeedbackServices _feedbackService;
    public FeedbackController(IFeedbackServices feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpPost]
    [Authorize]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
    public async Task<IActionResult> Property([FromForm] AddingFeedback feedback)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");

        var response = await _feedbackService.AddFeedback(feedback, uId);
        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

}
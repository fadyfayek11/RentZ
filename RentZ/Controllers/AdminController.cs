using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentZ.Application.Services.Admin;
using RentZ.DTO.Enums;
using RentZ.DTO.Response;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using RentZ.DTO.Property;
using RentZ.DTO.Feedback;

namespace RentZ.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,RootAdmin")]
public class AdminController : Controller
{
    private readonly IAdminServices _adminServices;

    public AdminController(IAdminServices adminServices)
    {
        _adminServices = adminServices;
    }

    [HttpPut(nameof(PropertyStatus))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
    public async Task<IActionResult> PropertyStatus(PropertyChangeStatus request)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");

        var response = await _adminServices.PropertyStatus(request, uId);
        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }
    
    [HttpGet(nameof(FeedBack))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<GettingFeedback?>>))]
    public async Task<IActionResult> FeedBack([FromQuery]Pagination request)
    {

        var response = await _adminServices.FeedBacks(request);
        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }
}
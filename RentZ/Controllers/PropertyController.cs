using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentZ.Application.Services.Property;
using RentZ.DTO.Enums;
using RentZ.DTO.Property;
using RentZ.DTO.Response;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

namespace RentZ.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PropertyController : Controller
{
    private readonly IPropertyService _propertyService;

    public PropertyController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<int>))]
    public async Task<IActionResult> Property([FromForm] AddingProperty property)
    {
        var response = await _propertyService.AddProperty(HttpContext, property);
        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<GetPropertyDetails?>))]
    public async Task<IActionResult> PropertyDetails([FromQuery]FindProperty propertyFilter)
    {
        var response = await _propertyService.GetProperty(HttpContext, propertyFilter);
        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    [HttpGet(nameof(Image))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(File))]
    public async Task<IActionResult> Image([FromQuery] PropImage image)
    {
        var response = await _propertyService.PropertyImage(image);

        if (response.Code is ErrorCode.InternalServerError) return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
        if (response.Code is ErrorCode.BadRequest || response.Data is null) return new BadRequestObjectResult(response);

        return File(await response.Data.ReadStreamAsync(), response.Message);
    }

    [HttpGet(nameof(GuestProperties))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<GetProperties?>>))]
    public async Task<IActionResult> GuestProperties([FromQuery] GuestPropertyFilter filter)
    {
        var response = await _propertyService.GetGuestProperties(HttpContext,filter);

        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    [Authorize]
    [HttpGet(nameof(Properties))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<GetProperties?>>))]
    public async Task<IActionResult> Properties([FromQuery] PropertyFilter filter)
    {
        var response = await _propertyService.GetProperties(HttpContext,filter);

        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    [Authorize(Roles = "Client")]
    [HttpGet(nameof(FavoriteProperties))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<GetProperties?>>))]
    public async Task<IActionResult> FavoriteProperties([FromQuery] Pagination filter)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");

        var response = await _propertyService.GetUserFavoriteProperties(uId, HttpContext, filter);

        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.Unauthorized) return new UnauthorizedObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    [HttpDelete]
    [Authorize(Roles = "Client")]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
    public async Task<IActionResult> Property([FromQuery] FindProperty filter)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");

        var response = await _propertyService.DeleteProperty(uId ?? "", filter);

        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.Unauthorized) return new UnauthorizedObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    [HttpPost(nameof(FavProperty))]
    [Authorize(Roles = "Client")]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
    public async Task<IActionResult> FavProperty([FromQuery] int propId)
    {
        var uId = HttpContext.User.FindFirstValue("UserId");

        var response = await _propertyService.FavoriteProperty(uId ?? "", propId);

        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    [EnableRateLimiting("property-view")]
    [HttpPatch(nameof(View))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<int>))]
    public async Task<IActionResult> View([FromQuery] FindProperty filter)
    {
        var response = await _propertyService.ViewProperty(filter);

        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }
}
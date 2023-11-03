using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentZ.Application.Services.Property;
using RentZ.DTO.Enums;
using RentZ.DTO.Property;
using RentZ.DTO.Response;
using Swashbuckle.AspNetCore.Annotations;

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
    [Authorize]
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
}
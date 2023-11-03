using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using RentZ.Application.Services.Property;
using RentZ.DTO.Enums;
using RentZ.DTO.JWT;
using RentZ.DTO.Property;
using RentZ.DTO.Response;
using RentZ.DTO.User.Security;
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
}
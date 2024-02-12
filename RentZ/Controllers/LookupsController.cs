using Microsoft.AspNetCore.Mvc;
using RentZ.Application.Services.Lookups;
using RentZ.DTO.Enums;
using RentZ.DTO.Lookups;
using RentZ.DTO.Response;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RentZ.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LookupsController : Controller
{   
    private readonly ILookupService _lookupService;
    public LookupsController(ILookupService lookupService)
    {
	    _lookupService = lookupService;
    }

    [HttpGet(nameof(Cities))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(List<LookupResponse>))]
    public async Task<IActionResult> Cities([FromQuery]LookupRequest lookup)
    {
	    var response = await _lookupService.GetCities(lookup);
	    return new OkObjectResult(response);
	}

    [HttpGet("Admin/Cities")]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(List<LookupResponseAdmin>))]
    public async Task<IActionResult> CitiesForAdmin([FromQuery]LookupRequest lookup)
    {
	    var response = await _lookupService.GetAdminCities(lookup);
	    return new OkObjectResult(response);
	}

    [Authorize]
    [HttpPatch(nameof(CityActivation))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool?>))]
    public async Task<IActionResult> CityActivation(int cityId)
    {
        var response = await _lookupService.CityActivation(cityId);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    } 
    
    [Authorize]
    [HttpPost(nameof(City))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool?>))]
    public async Task<IActionResult> City(AddLookup request)
    {
        var response = await _lookupService.AddCity(request);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }

    [HttpGet(nameof(Utilities))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(List<LookupResponse>))]
    public async Task<IActionResult> Utilities([FromQuery]LookupRequest lookup)
    {
	    var response = await _lookupService.GetUtilities(lookup);
	    return new OkObjectResult(response);
    }


    [HttpGet("Admin/Utilities")]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(List<LookupResponseAdmin>))]
    public async Task<IActionResult> UtilitiesForAdmin([FromQuery]LookupRequest lookup)
    {
	    var response = await _lookupService.GetAdminUtilities(lookup);
	    return new OkObjectResult(response);
    }

    [Authorize]
    [HttpPatch(nameof(UtilityActivation))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool?>))]
    public async Task<IActionResult> UtilityActivation(int utilityId)
    {
        var response = await _lookupService.UtilityActivation(utilityId);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }


    [Authorize]
    [HttpPost(nameof(Utility))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool?>))]
    public async Task<IActionResult> Utility(AddLookup request)
    {
        var response = await _lookupService.AddUtility(request);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }
}
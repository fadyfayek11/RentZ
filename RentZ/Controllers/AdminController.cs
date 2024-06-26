﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentZ.Application.Services.Admin;
using RentZ.DTO.Enums;
using RentZ.DTO.Response;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using RentZ.DTO.Property;
using RentZ.DTO.Feedback;
using RentZ.DTO.Lookups;
using RentZ.Application.Services.Lookups;
using RentZ.DTO.User.Security;
using DocumentFormat.OpenXml.Spreadsheet;

namespace RentZ.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,RootAdmin")]
public class AdminController : Controller
{
    private readonly IAdminServices _adminServices;
    private readonly ILookupService _lookupService;

    public AdminController(IAdminServices adminServices, ILookupService lookupService)
    {
        _adminServices = adminServices;
        _lookupService = lookupService;
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
    
    [HttpPut(nameof(EditCity))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
    public async Task<IActionResult> EditCity(UpdateLookup request)
    {

        var response = await _lookupService.UpdateCity(request);
        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    [HttpPut(nameof(EditUtility))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
    public async Task<IActionResult> EditUtility(UpdateLookup request)
    {

        var response = await _lookupService.UpdateUtility(request);
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


    [HttpGet(nameof(Cities))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(List<LookupResponseAdmin>))]
    public async Task<IActionResult> Cities([FromQuery] LookupRequest lookup)
    {
        var response = await _lookupService.GetAdminCities(lookup);
        return new OkObjectResult(response);
    }


    [HttpGet(nameof(Utilities))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(List<LookupResponseAdmin>))]
    public async Task<IActionResult> Utilities([FromQuery] LookupRequest lookup)
    {
        var response = await _lookupService.GetAdminUtilities(lookup);
        return new OkObjectResult(response);
    }


    [HttpPatch(nameof(UtilityActivation))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool?>))]
    public async Task<IActionResult> UtilityActivation(int utilityId)
    {
        var response = await _lookupService.UtilityActivation(utilityId);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }


    [HttpPost(nameof(Utility))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool?>))]
    public async Task<IActionResult> Utility(AddLookup request)
    {
        var response = await _lookupService.AddUtility(request);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }


    [HttpPatch(nameof(CityActivation))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool?>))]
    public async Task<IActionResult> CityActivation(int cityId)
    {
        var response = await _lookupService.CityActivation(cityId);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }


    [HttpPost(nameof(City))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool?>))]
    public async Task<IActionResult> City(AddLookup request)
    {
        var response = await _lookupService.AddCity(request);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }

    [Authorize(Roles = "RootAdmin")]
    [HttpPost(nameof(SubAdmin))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<AdminData?>))]
    public async Task<IActionResult> SubAdmin(SetAdminData request)
    {
        var response = await _adminServices.AddAdmin(request);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }

    [Authorize(Roles = "RootAdmin")]
    [HttpGet(nameof(SubAdmins))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<AdminData?>>))]
    public async Task<IActionResult> SubAdmins([FromQuery] RequestAdmin request)
    {

        var response = await _adminServices.GetAdmins(request);


        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    [Authorize(Roles = "RootAdmin")]
    [HttpPut(nameof(Update))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool?>))]
    public async Task<IActionResult> Update(UpdateAdminData request)
    {
        var response = await _adminServices.EditAdmin(request);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        if (response.Code is ErrorCode.InternalServerError) return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };

        return new OkObjectResult(response);
    }
    [HttpGet(nameof(Clients))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<PagedResult<AdminUserData?>>))]
    public async Task<IActionResult> Clients([FromQuery] RequestUsers request)
    {

        var response = await _adminServices.GetUsers(request);

        if (request.ExportData)
        {
            var fileContent = _adminServices.ExportUsersData(response.Data);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Clients.xlsx");
        }

        if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
        if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    [HttpPatch(nameof(Lock))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool?>))]
    public async Task<IActionResult> Lock(string userId)
    {
        var response = await _adminServices.LockUserAccount(userId);

        if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
    }
}
﻿using Microsoft.AspNetCore.Mvc;
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


    [HttpGet(nameof(Utilities))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(List<LookupResponse>))]
    public async Task<IActionResult> Utilities([FromQuery]LookupRequest lookup)
    {
	    var response = await _lookupService.GetUtilities(lookup);
	    return new OkObjectResult(response);
    }
}
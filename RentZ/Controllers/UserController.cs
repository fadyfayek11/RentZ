using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentZ.Application.Services.User.Security;
using RentZ.DTO.Enums;
using RentZ.DTO.JWT;
using RentZ.DTO.Response;
using RentZ.DTO.User.Security;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace RentZ.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : Controller
{
    private readonly IUserSecurityService _userSecurity;
    public UserController(IUserSecurityService userSecurity)
    {
        _userSecurity = userSecurity;
    }

	[HttpPost(nameof(Login))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<GenerateTokenResponseDto>))]
	public async Task<IActionResult> Login([FromBody] Login login)
	{
		var response = await _userSecurity.Login(login);
		if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
		if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }

	[HttpPost(nameof(Register))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<GenerateTokenResponseDto>))]
	public async Task<IActionResult> Register([FromBody] Registration register)
	{
		var response = await _userSecurity.Registration(register);
		if (response.Code == ErrorCode.Success) return new OkObjectResult(response);
		if (response.Code == ErrorCode.BadRequest) return new BadRequestObjectResult(response);

		return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
	}



    [Authorize]
    [HttpPost(nameof(VerifyOtp))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
    public async Task<IActionResult> VerifyOtp([FromBody] string otpNumber)
    {
	    var uId =  HttpContext.User.FindFirstValue("UserId");
	    var response = await _userSecurity.VerifyOtp(Guid.Parse(uId ?? "") , otpNumber);
       
	    if (response.Code is ErrorCode.BadRequest or ErrorCode.ValidationFailed) return new BadRequestObjectResult(response);
	    return new OkObjectResult(response);
    }

    //[Authorize]
    //[HttpPost(nameof(ResendOtp))]
    //[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse))]
    //public async Task<IActionResult> ResendOtp()
    //{
    //	var response = await _userService.ResendOtp();
    //	if (response.ErrorCode == 2) return new NotFoundObjectResult(response);
    //	if (response.ErrorCode == 4) return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };

    //	return new OkObjectResult(response);
    //}

}
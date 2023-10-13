using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentZ.Application.Services.User.Security;
using RentZ.DTO.Enums;
using RentZ.DTO.JWT;
using RentZ.DTO.Response;
using RentZ.DTO.User.Security;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using RentZ.Application.Services.Validations;

namespace RentZ.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : Controller
{
    private readonly IUserSecurityService _userSecurity;
    private readonly IValidations _validations;
    public UserController(IUserSecurityService userSecurity, IValidations validations)
    {
        _userSecurity = userSecurity;
        _validations = validations;
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

	[HttpPost(nameof(ValidatePhoneNumber))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
	public async Task<IActionResult> ValidatePhoneNumber([FromBody] string phoneNumber)
	{
		var response = await _validations.IsPhoneNumberExist(phoneNumber);
		if (response) return new BadRequestObjectResult(response);

		return new OkObjectResult(response);
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

	[Authorize]
	[HttpPost(nameof(ResendOtp))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
	public async Task<IActionResult> ResendOtp()
	{
        var uId = HttpContext.User.FindFirstValue("UserId");
        var response = await _userSecurity.ResendOtp(Guid.Parse(uId ?? ""));

        if (response.Code is ErrorCode.BadRequest or ErrorCode.FailOtp) return new BadRequestObjectResult(response);
        return new OkObjectResult(response);
	}

	[HttpPost(nameof(ForgetPassword))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
	public async Task<IActionResult> ForgetPassword(string phoneNumber)
	{
		var response = await _userSecurity.ForgetPasswordRequest(phoneNumber);

		if (response.Code is ErrorCode.BadRequest or ErrorCode.FailOtp) return new BadRequestObjectResult(response);
		return new OkObjectResult(response);
	}
}
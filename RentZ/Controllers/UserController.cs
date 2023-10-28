﻿using Microsoft.AspNetCore.Authorization;
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
    
    [HttpPost(nameof(ValidateEmail))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
	public async Task<IActionResult> ValidateEmail([FromBody] string email)
	{
		var response = await _validations.IsEmailExist(email);
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
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<GenerateTokenResponseDto?>))]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtp otp)
    {
	    var uId =  HttpContext.User.FindFirstValue("UserId");
	    var response = await _userSecurity.VerifyOtp(Guid.Parse(uId ?? "") , otp.OtpNumber);
       
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
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<GenerateTokenResponseDto?>))]
	public async Task<IActionResult> ForgetPassword(string phoneNumber)
	{
		var response = await _userSecurity.ForgetPasswordRequest(phoneNumber);

		if (response.Code is ErrorCode.BadRequest or ErrorCode.FailOtp) return new BadRequestObjectResult(response);
		return new OkObjectResult(response);
	}

	[Authorize]
	[HttpPost(nameof(ReSetPassword))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
	public async Task<IActionResult> ReSetPassword(string password)
	{
		var uId = HttpContext.User.FindFirstValue("UserId");
		var response = await _userSecurity.SetPassword(new SetPassword(uId, password));

		if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
		return new OkObjectResult(response);
	}
	
	[Authorize]
	[HttpPost(nameof(ChangePassword))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
	public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
	{
		var uId = HttpContext.User.FindFirstValue("UserId");
		var response = await _userSecurity.ChangePassword(new ChangePassword(uId, oldPassword, newPassword));

		if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
		return new OkObjectResult(response);
	}
	
	[Authorize]
	[HttpPost(nameof(ChangeLanguage))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
	public async Task<IActionResult> ChangeLanguage(string favLang)
	{
		var uId = HttpContext.User.FindFirstValue("UserId");
		var response = await _userSecurity.ChangeLanguage(new SetLanguage(uId, favLang));

		if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
		return new OkObjectResult(response);
	}
    
    [Authorize]
	[HttpGet(nameof(UserInfo))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<UserData?>))]
	public async Task<IActionResult> UserInfo()
	{
		var uId = HttpContext.User.FindFirstValue("UserId");
        var response = await _userSecurity.UserInformation(uId, HttpContext);

		if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
		return new OkObjectResult(response);
	}
    
    [Authorize]
	[HttpPost(nameof(ProfilePic))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<string?>))]
	public async Task<IActionResult> ProfilePic(IFormFile image)
	{
		var uId = HttpContext.User.FindFirstValue("UserId");
		var response = await _userSecurity.ProfileImage(uId, image, HttpContext);

		if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
		return new OkObjectResult(response);
	}
    
    [Authorize]
	[HttpDelete(nameof(ProfilePic))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<bool>))]
	public async Task<IActionResult> ProfilePic()
	{
		var uId = HttpContext.User.FindFirstValue("UserId");
		var response = await _userSecurity.DeleteProfileImage(uId);

		if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
		return new OkObjectResult(response);
	}
    
    [Authorize]
	[HttpPut(nameof(UpdateProfilePic))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<string?>))]
	public async Task<IActionResult> UpdateProfilePic(IFormFile image)
	{
		var uId = HttpContext.User.FindFirstValue("UserId");
		var response = await _userSecurity.UpdateProfileImage(uId, image, HttpContext);

		if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
		return new OkObjectResult(response);
	}

    [Authorize]
	[HttpPut(nameof(UserInfo))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<GenerateTokenResponseDto?>))]
    public async Task<IActionResult> UserInfo(EditUserData userData)
	{
		var uId = HttpContext.User.FindFirstValue("UserId");
		var response = await _userSecurity.EditUserInformation(uId, userData);

		if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
		return new OkObjectResult(response);
	}

    [Authorize]
	[HttpPatch(nameof(PhoneNumber))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<GenerateTokenResponseDto?>))]
    public async Task<IActionResult> PhoneNumber(ChangePhoneNumber number)
	{
		var uId = HttpContext.User.FindFirstValue("UserId");
		var response = await _userSecurity.ChangePhoneNumber(uId, number.NewPhoneNumber);

		if (response.Code is ErrorCode.BadRequest) return new BadRequestObjectResult(response);
		return new OkObjectResult(response);
	}
    
	[HttpGet(nameof(Profile))]
	[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(File))]
    public async Task<IActionResult> Profile([FromQuery] Guid uId)
	{
        var response = await _userSecurity.Profile(uId.ToString());

        if (response.Code is ErrorCode.BadRequest || response.Data is null) return new BadRequestObjectResult(response);
        return File(await response.Data.ReadStreamAsync(), response.Message);
	}

}
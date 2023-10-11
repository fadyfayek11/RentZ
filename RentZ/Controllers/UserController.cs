using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentZ.Application.Services.User.Security;
using Swashbuckle.AspNetCore.Annotations;

namespace RentZ.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : Controller
{
    //[HttpPost(nameof(Login))]
	//[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(LoginDto))]
	//public async Task<IActionResult> Login([FromBody] UserTokenRequest user)
	//{
	//	var response = await _userService.Login(user.UserIqamaId, user.PhoneNumber);
	//	if (response.ErrorCode == 1) return new BadRequestObjectResult(response);
	//	if (response.ErrorCode == 3) return new UnauthorizedObjectResult(response);
	//	if (response.ErrorCode == 4) return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
	//	if (response.ErrorCode == 6) return new BadRequestObjectResult(response);

	//	return new OkObjectResult(response);
	//}

	//[Authorize]
	//[HttpPost(nameof(VerifyOtp))]
	//[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(BaseResponse<>))]
	//public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest otp)
	//{
	//	var response = await _userService.VerifyOtp(otp.OtpCode);
	//	if (response.ErrorCode == 1) return new BadRequestObjectResult(response);
	//	if (response.ErrorCode == 2) return new NotFoundObjectResult(response);

	//	return new OkObjectResult(response);
	//}

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
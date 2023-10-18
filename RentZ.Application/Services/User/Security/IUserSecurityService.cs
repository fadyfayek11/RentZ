using Microsoft.AspNetCore.Http;
using RentZ.DTO.JWT;
using RentZ.DTO.Response;
using RentZ.DTO.User.Security;

namespace RentZ.Application.Services.User.Security
{
    public interface IUserSecurityService
    {
        Task<BaseResponse<GenerateTokenResponseDto>> Login(Login login);
        Task<BaseResponse<GenerateTokenResponseDto>> Registration(Registration register);
        Task<BaseResponse<GenerateTokenResponseDto?>> VerifyOtp(Guid userId, string otpNumber);
		Task<BaseResponse<bool>> ResendOtp(Guid userId);
		Task<BaseResponse<GenerateTokenResponseDto?>> ForgetPasswordRequest(string phoneNumber);
		Task<BaseResponse<bool>> SetPassword(SetPassword password);
		Task<BaseResponse<bool>> ChangePassword(ChangePassword password);
		Task<BaseResponse<bool>> ChangeLanguage(SetLanguage lang);
		Task<BaseResponse<UserData?>> UserInformation(string userId);
		Task<BaseResponse<bool>> ProfileImage(string userId, IFormFile image);

	}
}

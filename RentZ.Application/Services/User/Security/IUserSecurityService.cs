using ExtCore.FileStorage.Abstractions;
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
		Task<BaseResponse<UserData?>> UserInformation(string userId, HttpContext context);
		Task<BaseResponse<GenerateTokenResponseDto?>> EditUserInformation(string userId, EditUserData userDate);
		Task<BaseResponse<string?>> ProfileImage(string userId, IFormFile image, HttpContext context);
        Task<BaseResponse<IFileProxy?>> Profile(string userId);
        Task<BaseResponse<bool>> DeleteProfileImage(string userId);
        Task<BaseResponse<string?>> UpdateProfileImage(string userId, IFormFile image, HttpContext context);

    }
}

using RentZ.DTO.JWT;
using RentZ.DTO.Response;
using RentZ.DTO.User.Security;

namespace RentZ.Application.Services.User.Security
{
    public interface IUserSecurityService
    {
        Task<BaseResponse<GenerateTokenResponseDto>> Login(Login login);
        Task<BaseResponse<GenerateTokenResponseDto>> Registration(Registration register);
        Task<BaseResponse<bool>> VerifyOtp(Guid userId, string otpNumber);
        Task<BaseResponse<string>> ResendOtp(Guid userId);
    }
}

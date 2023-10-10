using RentZ.DTO.JWT;
using RentZ.DTO.Logging;
using RentZ.DTO.User.Security;

namespace RentZ.Application.Services.User.Security
{
    public interface IUserSecurityService
    {
        Task<BaseResponse<GenerateTokenResponseDto>> Login(Login login);
        Task<BaseResponse<bool>> VerifyOtp(string otpNumber);
        Task<BaseResponse<string>> ResendOtp();
    }
}

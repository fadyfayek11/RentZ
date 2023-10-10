using Microsoft.AspNetCore.Identity;
using RentZ.Application.Services.JWT;
using RentZ.DTO.JWT;
using RentZ.DTO.Logging;
using RentZ.DTO.User.Security;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.User.Security
{
    public class UserSecurityService : IUserSecurityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
		public UserSecurityService(IJwtService jwtService, ApplicationDbContext context)
        {
	        _jwtService = jwtService;
	        _context = context;

        }
		public async Task<BaseResponse<GenerateTokenResponseDto>> Login(Login login)
        {
            UserManager

	        var tokenResult = _jwtService.GenerateToken(new GenerateTokenRequestDto());


            
            var validOtp = await SetOtp(userIqamaId, phoneNumber);

            if (!validOtp)
                return new LoginDto()
                {
                    ErrorCode = 4,
                    Message = "Error while sending the otp",
                };

            var result = new LoginDto
            {
                ErrorCode = 0,
                Message = string.Empty,
                LoginData = new UserLoginDto
                {
                    Token = accessToken,
                    TokenExpirationDate = (DateTime)accessTokenExpiryDate!,
                    RefreshToken = refreshToken,
                    RefreshTokenExpirationDate = (DateTime)refreshTokenExpiryDate!,
                    UserData = new UserData
                    {
                        UserId = userIqamaId,
                        UserName = member.E_NAME,
                        PhoneNumber = originalPhoneNumber,
                    }
                }
            };
            return result;
        }

		public async Task<BaseResponse<bool>> VerifyOtp(string uId, string otpNumber)
        {
            
        }
       
        private static string GenerateCode(int length = 4)
        {
            var maxNumber = (int)Math.Pow(10, length) - 1;
            var otp = new Random().Next(0, maxNumber);

            var otpString = otp.ToString().PadLeft(length, '0');

            return otpString;
        }
        private async Task<bool> SetOtp(string IqamaNo,string MobileNo)
        {
            
        }
        public async Task<BaseResponse<string>> ResendOtp()
        {
        }
      

    }
}

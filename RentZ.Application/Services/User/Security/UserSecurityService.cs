using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentZ.Application.Services.JWT;
using RentZ.Domain.Entities;
using RentZ.DTO.Enums;
using RentZ.DTO.JWT;
using RentZ.DTO.Response;
using RentZ.DTO.User.Security;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.User.Security
{
    public class UserSecurityService : IUserSecurityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly UserManager<Domain.Entities.User> _userManager;
		public UserSecurityService(IJwtService jwtService, ApplicationDbContext context, UserManager<Domain.Entities.User> userManager)
        {
	        _jwtService = jwtService;
	        _context = context;
            _userManager = userManager;
        }
		public async Task<BaseResponse<GenerateTokenResponseDto>> Login(Login login)
        {
            var user = await _userManager.FindByNameAsync(login.UserName);
            if (user == null)
            {
                return new BaseResponse<GenerateTokenResponseDto>(){ Code = ErrorCode.BadRequest , Message = "User Name or pass may be wrong"};
            }

            var correctPass = await _userManager.CheckPasswordAsync(user, login.Password);
            if (!correctPass)
            {
                return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.BadRequest, Message = "User Name or pass may be wrong" };
            }
            
            var client = await _context.Clients.FirstOrDefaultAsync(x=>x.Id == user.Id);
            if (client == null)
            {
                return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.BadRequest, Message = "User Name or pass may be wrong" };
            }

            var tokenResult = _jwtService.GenerateToken(new GenerateTokenRequestDto(user.Id.ToString(), user.UserName, user.Email, user.PhoneNumber, client.Gender, client.Bio, client.FavLang, client.City.Name, user.IsActive, user.PhoneNumberConfirmed));

            return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.FailOtp, Message = "Success Get Token but fail to send otp", Data = tokenResult};
        }

        public Task<BaseResponse<bool>> VerifyOtp(string otpNumber)
        {
            throw new NotImplementedException();
        }
        private static string GenerateCode(int length = 4)
        {
            var maxNumber = (int)Math.Pow(10, length) - 1;
            var otp = new Random().Next(0, maxNumber);

            var otpString = otp.ToString().PadLeft(length, '0');

            return otpString;
        }
        private async Task<bool> SetOtp(Guid userId)
        {
            var code = GenerateCode();
            _context.OtpSetups.Update(new OtpSetup()
            {
                Id = userId,
                Code = code,
                ExpiryDate = DateTime.Now.AddMinutes(2)
            });

            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<BaseResponse<string>> ResendOtp()
        {
            throw new NotImplementedException();

        }


    }
}

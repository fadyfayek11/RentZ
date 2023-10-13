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
            //ToDo: Fluent Validation middleware

            var user = await _userManager.FindByNameAsync(login.PhoneNumber);
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

            var tokenResult = GenerateToken(user, client);

            return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.Success, Message = "Success Get Token", Data = tokenResult};
        }
        public async Task<BaseResponse<GenerateTokenResponseDto>> Registration(Registration register)
        {
            //ToDo: Fluent Validation middleware
            var newUser = new Domain.Entities.User
            {
                UserName = register.PhoneNumber,
                NormalizedUserName = register.PhoneNumber.ToUpper(),
                Email = register.UserEmail,
                NormalizedEmail = register.UserEmail.ToUpper(),
                EmailConfirmed = false,
                PasswordHash = null,
                SecurityStamp = null,
                ConcurrencyStamp = null,
                PhoneNumber = register.PhoneNumber,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnd = null,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                DisplayNameName = register.DisplayName,
                IsActive = true
            };
           
            var result = await _userManager.CreateAsync(newUser, register.Password);
            if(!result.Succeeded)
                return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.InternalServerError, Message = "Something went wrong while saving the data" };

            var userInRole = await _userManager.AddToRoleAsync(newUser, "Client");
            if (!userInRole.Succeeded)
	            return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.InternalServerError, Message = "Something went wrong while add user to role" };

			var newClient = new Client
            {
                Id = newUser.Id,
                IsOwner = register.IsOwner,
                Gender = (Gender)Enum.Parse(typeof(Gender),register.Gender),
                BirthDate = register.BirthDate,
                ProfileImage = null,
                FavLang = (Lang)Enum.Parse(typeof(Lang), register.FavLang),
                Bio = null,
                CityId = register.CityId,
                User = newUser,
                City = await _context.City.FirstOrDefaultAsync(x=>x.Id == register.CityId)
            };
            await _context.Clients.AddAsync(newClient);
            var savedSuccessfully = await _context.SaveChangesAsync() > 0;

            if (!savedSuccessfully)
            {
                return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.InternalServerError, Message = "Something went wrong while saving the data" };
            }

            var (successSetOtp, tokenResult) = await SetOtp(newUser);
            return new BaseResponse<GenerateTokenResponseDto>() { Code = successSetOtp? ErrorCode.Success : ErrorCode.FailOtp, Message = "Success Get Token", Data = tokenResult };
        }
        private GenerateTokenResponseDto GenerateToken(Domain.Entities.User user, Client client)
        {
            var tokenResult = _jwtService.GenerateToken(new GenerateTokenRequestDto(user.Id.ToString(), user.UserName,
                user.Email, user.PhoneNumber, client.Gender, client.Bio, client.FavLang,
                client.City?.Name ?? "", client.IsOwner, user.IsActive, 
                user.PhoneNumberConfirmed,Roles.Client));
            return tokenResult;
        }
        public async Task<BaseResponse<bool>> VerifyOtp(Guid userId, string otpNumber)
        {
            var userOtp = await _context.OtpSetups.FirstOrDefaultAsync(x => x.Id == userId);

            if (userOtp == null)
                return new BaseResponse<bool>() { Code = ErrorCode.BadRequest, Message = "Can't verify", Data = false};

            if (userOtp.ExpiryDate <= DateTime.UtcNow)
                return new BaseResponse<bool>() { Code = ErrorCode.ValidationFailed, Message = "Otp time out", Data = false};

            if (userOtp.Code != otpNumber)
                return new BaseResponse<bool>() { Code = ErrorCode.ValidationFailed, Message = "Wrong otp number", Data = false};

            var user = await _context.Users.FirstOrDefaultAsync(x=>x.Id == userId);
            user!.PhoneNumberConfirmed = true;
            
            _context.Update(user);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>() { Code = ErrorCode.Success, Message = "Verification done successfully", Data = true};
        }
        //private static string GenerateCode(int length = 4)
        //{
        //    var maxNumber = (int)Math.Pow(10, length) - 1;
        //    var otp = new Random().Next(0, maxNumber);

        //    var otpString = otp.ToString().PadLeft(length, '0');

        //    return otpString;
        //}
        private async Task<(bool, GenerateTokenResponseDto)> SetOtp(Domain.Entities.User user)
        {
            var userOtp = await _context.OtpSetups.FirstOrDefaultAsync(x => x.Id == user.Id);
			
            //var code = GenerateCode();
			var code = "1234";
			if (userOtp == null)
            {
	            await _context.OtpSetups.AddAsync(new OtpSetup()
	            {
		            Id = user.Id,
		            Code = code,
		            ExpiryDate = DateTime.Now.AddMinutes(2)
	            });
            }
			else
			{
				userOtp.Code = code;
				userOtp.ExpiryDate = DateTime.Now.AddMinutes(2);

				_context.OtpSetups.Update(userOtp);
			}

			var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == user.Id);
            if(client is null)
                return (false, null)!;

			var tokenResult = GenerateToken(user, client);

			//ToDo: making integration with orange to send otp sms
			return (await _context.SaveChangesAsync() > 0, tokenResult);
        }
        public async Task<BaseResponse<GenerateTokenResponseDto?>> ResendOtp(Guid userId)
        {
	        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if(user is null)
	            return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.FailOtp, Message = "Fail to send otp", Data = null };


			var (successSetOtp, token) = await SetOtp(user);
	        return new BaseResponse<GenerateTokenResponseDto?>() { Code = successSetOtp ? ErrorCode.Success : ErrorCode.FailOtp, Message = successSetOtp ? "Success send otp" : "Fail to send otp", Data = token };
        }
        public async Task<BaseResponse<GenerateTokenResponseDto?>> ForgetPasswordRequest(string phoneNumber)
        {
	        var user =  await _userManager.FindByNameAsync(phoneNumber);
	        if (user is null) return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.BadRequest, Message = "Can't find the user", Data = null };

			var (successSetOtp, token) = await SetOtp(user);
			return new BaseResponse<GenerateTokenResponseDto?>() { Code = successSetOtp ? ErrorCode.Success : ErrorCode.FailOtp, Message = successSetOtp ? "Success send otp" : "Fail to send otp", Data = token };
        }
        public async Task<BaseResponse<bool>> SetPassword(SetPassword password)
        {
	        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(password.UserId));
	        if(user is null)
				return new BaseResponse<bool>() { Code = ErrorCode.BadRequest, Message = "Fail set the password", Data = false };

            var passToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, passToken, password.NewPassword);

            return new BaseResponse<bool>() { Code = result.Succeeded ? ErrorCode.Success : ErrorCode.BadRequest, Message = result.Succeeded ? "Success to set password" : "Fail to set password", Data = result.Succeeded };
        }
	}
}

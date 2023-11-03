using ExtCore.FileStorage.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentZ.Application.Services.Files;
using RentZ.Application.Services.JWT;
using RentZ.Domain.Entities;
using RentZ.DTO.Enums;
using RentZ.DTO.JWT;
using RentZ.DTO.Lookups;
using RentZ.DTO.Response;
using RentZ.DTO.User.Security;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.User.Security
{
    public class UserSecurityService : IUserSecurityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IFileManager _fileManager;
        private readonly UserManager<Domain.Entities.User> _userManager;
		public UserSecurityService(IJwtService jwtService, ApplicationDbContext context, UserManager<Domain.Entities.User> userManager, IFileManager fileManager)
        {
	        _jwtService = jwtService;
	        _context = context;
            _userManager = userManager;
            _fileManager = fileManager;
        }

		public async Task<BaseResponse<GenerateTokenResponseDto>> Login(Login login, HttpContext context)
        {
            //ToDo: Fluent Validation middleware

            var user = await _userManager.FindByNameAsync(login.PhoneNumber);
            if (user == null)
            {
                return new BaseResponse<GenerateTokenResponseDto>(){ Code = ErrorCode.BadRequest , Message = "User Name or pass may be wrong"};
            }

            if (!user.IsActive)
            {
	            return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.BadRequest, Message = "User Name or pass may be wrong" };
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

            var tokenResult = GenerateToken(user, client, context);

            return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.Success, Message = "Success Get Token", Data = tokenResult};
        }
        public async Task<BaseResponse<GenerateTokenResponseDto>> Registration(Registration register, HttpContext context)
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
                DisplayName = register.DisplayName,
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

            var (successSetOtp, tokenResult) = await SetOtp(newUser, context);
            return new BaseResponse<GenerateTokenResponseDto>() { Code = successSetOtp? ErrorCode.Success : ErrorCode.FailOtp, Message = "Success Get Token", Data = tokenResult };
        }
        private GenerateTokenResponseDto GenerateToken(Domain.Entities.User user, Client client, HttpContext context)
        {
            var tokenResult = _jwtService.GenerateToken(new GenerateTokenRequestDto(user.Id.ToString(), user.DisplayName,
                user.Email, !string.IsNullOrEmpty(client.ProfileImage) ? GetProfileImageUrl(user.Id.ToString(), context) : "", user.PhoneNumber, client.Gender,client.FavLang, client.IsOwner, user.IsActive, 
                user.PhoneNumberConfirmed,Roles.Client));
            return tokenResult;
        }
        public async Task<BaseResponse<GenerateTokenResponseDto?>> VerifyOtp(Guid userId, string otpNumber, HttpContext context)
        {
            var userOtp = await _context.OtpSetups.FirstOrDefaultAsync(x => x.Id == userId);

            if (userOtp == null)
                return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.BadRequest, Message = "Can't verify", Data = null};

            if (userOtp.ExpiryDate <= DateTime.UtcNow)
                return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.ValidationFailed, Message = "Otp time out", Data = null};

            if (userOtp.Code != otpNumber)
                return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.ValidationFailed, Message = "Wrong otp number", Data = null};

            var user = await _context.Users.FirstOrDefaultAsync(x=>x.Id == userId);
            var client = await _context.Clients.FirstOrDefaultAsync(x=>x.Id == userId);
           
            if (client is null || user is null)
                return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.BadRequest, Message = "Can't verify", Data = null };

            user.PhoneNumberConfirmed = true;
            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var tokenResult = GenerateToken(user, client, context);
            return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.Success, Message = "Verification done successfully", Data = tokenResult };
        }
        //private static string GenerateCode(int length = 4)
        //{
        //    var maxNumber = (int)Math.Pow(10, length) - 1;
        //    var otp = new Random().Next(0, maxNumber);

        //    var otpString = otp.ToString().PadLeft(length, '0');

        //    return otpString;
        //}
        private async Task<(bool, GenerateTokenResponseDto)> SetOtp(Domain.Entities.User user, HttpContext context)
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

			var tokenResult = GenerateToken(user, client, context);

			//ToDo: making integration with orange to send otp sms
			return (await _context.SaveChangesAsync() > 0, tokenResult);
        }
        public async Task<BaseResponse<bool>> ResendOtp(Guid userId, HttpContext context)
        {
	        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if(user is null)
	            return new BaseResponse<bool>() { Code = ErrorCode.FailOtp, Message = "Fail to send otp", Data = false };


			var (successSetOtp, _) = await SetOtp(user, context);
	        return new BaseResponse<bool>() { Code = successSetOtp ? ErrorCode.Success : ErrorCode.FailOtp, Message = successSetOtp ? "Success send otp" : "Fail to send otp", Data = successSetOtp };
        }
        public async Task<BaseResponse<GenerateTokenResponseDto?>> ForgetPasswordRequest(string phoneNumber, HttpContext context)
        {
	        var user =  await _userManager.FindByNameAsync(phoneNumber);
	        if (user is null) return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.BadRequest, Message = "Can't find the user", Data = null };

			var (successSetOtp, token) = await SetOtp(user, context);
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
        public async Task<BaseResponse<bool>> ChangePassword(ChangePassword password)
        {
	        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(password.UserId));
	        if (user is null)
		        return new BaseResponse<bool>() { Code = ErrorCode.BadRequest, Message = "Fail change the password", Data = false };

	        var result = await _userManager.ChangePasswordAsync(user, password.OldPassword, password.NewPassword);

	        return new BaseResponse<bool>() { Code = result.Succeeded ? ErrorCode.Success : ErrorCode.BadRequest, Message = result.Succeeded ? "Success to change password" : "Fail to change password", Data = result.Succeeded };
        }
        public async Task<BaseResponse<bool>> ChangeLanguage(SetLanguage lang)
        {
			var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == Guid.Parse(lang.UserId));
			if (client is null)
				return new BaseResponse<bool>() { Code = ErrorCode.BadRequest, Message = "Fail change the language", Data = false };
			
			client.FavLang = (Lang)Enum.Parse(typeof(Lang), lang.Lang);
			_context.Clients.Update(client);
			await _context.SaveChangesAsync();

			return new BaseResponse<bool>() { Code = ErrorCode.Success, Message = "Change the language done successfully", Data = true };
        }
        public async Task<BaseResponse<UserData?>> UserInformation(string userId, HttpContext context)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            if (client is null)
                return new BaseResponse<UserData?>() { Code = ErrorCode.BadRequest, Message = "Fail to get user data", Data = null };

            var favLang = client.FavLang;
            var userDataResponse = new UserData(userId,!string.IsNullOrEmpty(client.ProfileImage) ?GetProfileImageUrl(userId, context) : null, client.User.DisplayName, client.User.Email, client.User.PhoneNumber,
                favLang.ToString(), client.BirthDate,
                new LookupResponse() { Id = client.CityId, Value = favLang == Lang.en? client.City.NameEn : client.City.Name },
                new LookupResponse() { Id = client.City.GovernorateId, Value = favLang == Lang.en ? client.City.Governorate.NameEn : client.City.Governorate.Name },
                client.Gender.ToString(), client.IsOwner, client.User.IsActive, client.User.PhoneNumberConfirmed);
           
            return new BaseResponse<UserData?>() { Code = ErrorCode.Success, Message = "get user data done successfully", Data = userDataResponse };
        }
        public async Task<BaseResponse<GenerateTokenResponseDto?>> EditUserInformation(string userId, EditUserData userDate, HttpContext context)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            if (client is null)
                return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.BadRequest, Message = "Fail to edit user data", Data = null };
            
            var user = client.User;
            if (user is null)
                return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.BadRequest, Message = "Fail to edit user data", Data = null };

            user.DisplayName = userDate.DisplayName ?? user.DisplayName;
            user.Email = userDate.Email ?? user.Email;
            client.CityId = userDate.CityId ?? client.CityId;
            client.BirthDate = userDate.BirthDate ?? client.BirthDate;
            client.Gender = string.IsNullOrEmpty(userDate.Gender) ? client.Gender : (Gender)Enum.Parse(typeof(Gender),userDate.Gender);

			var tokenResult = GenerateToken(user, client, context);
            bool successSetOtp = !string.IsNullOrEmpty(tokenResult.Token);

			_context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return new BaseResponse<GenerateTokenResponseDto?>() { Code = successSetOtp ? ErrorCode.Success : ErrorCode.FailOtp, Message = successSetOtp ? "Success to edit user data" : "Fail to edit user data", Data = tokenResult };

        }
        public async Task<BaseResponse<string?>> ProfileImage(string userId, IFormFile image, HttpContext context)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            if (client is null)
                return new BaseResponse<string?>() { Code = ErrorCode.BadRequest, Message = "Fail to upload user profile pic", Data = null };

            var fileName = $"{Guid.NewGuid()}-{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(image.FileName)}";
            var saved = await _fileManager.SaveFileAsync<Domain.Entities.User>(image, fileName, userId);
            if (!saved)
                return new BaseResponse<string?>() { Code = ErrorCode.BadRequest, Message = "Fail to upload user profile pic", Data = null };


            client.ProfileImage = fileName;
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return new BaseResponse<string?>() { Code = ErrorCode.Success, Message = "Upload user profile pic done successfully", Data = GetProfileImageUrl(userId, context) };
        }
        public async Task<BaseResponse<bool>> DeleteProfileImage(string userId)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            if (client is null)
                return new BaseResponse<bool>() { Code = ErrorCode.BadRequest, Message = "Fail to remove user profile pic", Data = false };
           
            client.ProfileImage = string.Empty;
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
           
            return new BaseResponse<bool>() { Code = ErrorCode.Success, Message = "Removing your profile image done successfully", Data = true };
        }
        public async Task<BaseResponse<string?>> UpdateProfileImage(string userId, IFormFile image, HttpContext context)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            if (client is null)
                return new BaseResponse<string?>() { Code = ErrorCode.BadRequest, Message = "Fail to update user profile pic", Data = null };

            var newFileName = $"{Guid.NewGuid()}-{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(image.FileName)}";

            var saved = await _fileManager.UpdateFileAsync<Domain.Entities.User>(image, client.ProfileImage, newFileName, userId);
            if (!saved)
                return new BaseResponse<string?>() { Code = ErrorCode.BadRequest, Message = "Fail to upload user profile pic", Data = null };


            client.ProfileImage = newFileName;
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return new BaseResponse<string?>() { Code = ErrorCode.Success, Message = "Update user profile pic done successfully", Data = GetProfileImageUrl(userId, context) };
        }
        public async Task<BaseResponse<GenerateTokenResponseDto?>> ChangePhoneNumber(string userId, string newNumber, HttpContext context)
        {
	        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
			if (user is null) return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.BadRequest, Message = "Can't find the user", Data = null };
			
			if (user.PhoneNumber == newNumber) return new BaseResponse<GenerateTokenResponseDto?>() { Code = ErrorCode.BadRequest, Message = "You entered the same number, please enter different one", Data = null };

            user.UserName = newNumber;
            user.PhoneNumber = newNumber;
			user.PhoneNumberConfirmed = false;

			_context.Users.Update(user);
			await _context.SaveChangesAsync();

			var (successSetOtp, token) = await SetOtp(user, context);
			return new BaseResponse<GenerateTokenResponseDto?>() { Code = successSetOtp ? ErrorCode.Success : ErrorCode.FailOtp, Message = successSetOtp ? "Success send otp" : "Fail to send otp", Data = token };
		}

        public async Task<BaseResponse<bool?>> AccountActivity(string userId)
        {
	        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
	        if (user is null)
		        return new BaseResponse<bool?>() { Code = ErrorCode.BadRequest, Message = "Fail to get user data", Data = null };

	        user.IsActive = !user.IsActive;
	        _context.Users.Update(user);
	        await _context.SaveChangesAsync();

			return new BaseResponse<bool?>() { Code = ErrorCode.Success, Message = user.IsActive? "Activate user account done" : "Deactivate user account done", Data = user.IsActive };
        }

		public async Task<BaseResponse<IFileProxy?>> Profile(string userId)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            if (client is null)
                return new BaseResponse<IFileProxy?>() { Code = ErrorCode.BadRequest, Message = "Fail to get user profile pic", Data = null };

            if (string.IsNullOrEmpty(client.ProfileImage))
	            return new BaseResponse<IFileProxy?>() { Code = ErrorCode.InternalServerError, Message = "User hadn't set a pic yet", Data = null };

			var fileImage = await _fileManager.FileProxy<Domain.Entities.User>(client.ProfileImage, userId);
            var contentType = _fileManager.GetContentType(fileImage.Filename);
            return new BaseResponse<IFileProxy?>() { Code = ErrorCode.Success, Message = contentType, Data = fileImage };
        }
        private string GetProfileImageUrl(string uId,HttpContext context)
        {
            var request = context.Request;

            var scheme = request.Scheme;

            var host = request.Host.Value;

            var url = $"{scheme}://{host}/api/User/Profile?uId={uId}";

            return url;
        }
    }
}

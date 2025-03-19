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
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RentZ.Application.DtoValidations;

namespace RentZ.Application.Services.User.Security
{
    public class UserSecurityService : IUserSecurityService
    {
        private readonly ILogger<UserSecurityService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IFileManager _fileManager;
        private readonly UserManager<Domain.Entities.User> _userManager;
        private readonly byte[] _encryptionKey = Encoding.UTF8.GetBytes("9B31C3F0A4E525CCD6AD1D24FBCB438E106E53E2391FC179");
        public UserSecurityService(IJwtService jwtService, ApplicationDbContext context, UserManager<Domain.Entities.User> userManager, IFileManager fileManager, ILogger<UserSecurityService> logger)
        {
	        _jwtService = jwtService;
	        _context = context;
            _userManager = userManager;
            _fileManager = fileManager;
            _logger = logger;
        }

		public async Task<BaseResponse<GenerateTokenResponseDto>> Login(Login login, HttpContext context)
        {
            //ToDo: Fluent Validation middleware

            var user = await _context.Users.FirstOrDefaultAsync(x=> x.PhoneNumber == login.PhoneNumber || x.UserName == login.PhoneNumber);
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
            
            var client = await _context.Clients.FirstOrDefaultAsync(x=>x.Id == user.Id) ?? new Client();

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
                return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.BadRequest, Message = "Something went wrong while saving the data" };

            var userInRole = await _userManager.AddToRoleAsync(newUser, "Client");
            if (!userInRole.Succeeded)
	            return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.BadRequest, Message = "Something went wrong while add user to role" };

            await _userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Sid, newUser.Id.ToString()));
            var newClient = new Client
            {
                Id = newUser.Id,
                IsOwner = register.IsOwner,
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
                return new BaseResponse<GenerateTokenResponseDto>() { Code = ErrorCode.BadRequest, Message = "Something went wrong while saving the data" };
            }

            var (successSetOtp, tokenResult) = await SetOtp(newUser, context);
            return new BaseResponse<GenerateTokenResponseDto>() { Code = successSetOtp? ErrorCode.Success : ErrorCode.FailOtp, Message = "Success Get Token", Data = tokenResult };
        }
        private GenerateTokenResponseDto GenerateToken(Domain.Entities.User user, Client client, HttpContext context)
        {
            var tokenResult = _jwtService.GenerateToken(new GenerateTokenRequestDto(user.Id.ToString(), user.DisplayName,
                user.Email, GetProfileImageUrl(user.Id.ToString(), context), user.PhoneNumber, client.FavLang, client.IsOwner, user.IsActive, 
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
       
        private static string GenerateCode(int length = 4)
        {
            var maxNumber = (int)Math.Pow(10, length) - 1;
            var otp = new Random().Next(0, maxNumber);

            var otpString = otp.ToString().PadLeft(length, '0');

            return otpString;
        }
        private async Task<bool> SendOtpViaOrangeApi(string phoneNumber, string otpCode)
        {
            var apiUrl = "https://marketingportal.access2arabia.com:7755/JSON/API/A2A/SendSMS";
            var username = "Berveh24";
            var password = "Berveh24@Orange227";
            var senderId = "Berveh"; 

            var smsText = $"Your OTP code is {otpCode}";

            var requestBody = new
            {
                BankCode = username,
                BankPWD = password,
                SenderID = senderId,
                MsgText = smsText,
                MobileNo = phoneNumber
            };

            using (var httpClient = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await httpClient.PostAsync(apiUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<OrangeApiResponse>(responseContent);

                        if (result.ErrorCode == 0)
                        {
                            return true;
                        }

                        _logger.LogError($"Orange API Error: {result.ErrorMessage}");
                        return false;
                    }

                    _logger.LogError($"Orange API Request Failed: {response.StatusCode}");
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception while calling Orange API: {ex.Message}");
                    return false;
                }
            }
        }
        private async Task<(bool, GenerateTokenResponseDto)> SetOtp(Domain.Entities.User user, HttpContext context)
        {
            var userOtp = await _context.OtpSetups.FirstOrDefaultAsync(x => x.Id == user.Id);
			
            var code = GenerateCode();
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
            if (client is not null)
            {
                var tokenResult = GenerateToken(user, client, context);

                var phone = user.PhoneNumber.Split('+')[1];
                await SendOtpViaOrangeApi(phone, code);

                return (await _context.SaveChangesAsync() > 0, tokenResult);
            }
            
            
            return (false, null)!;
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
            var user = await _context.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
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
            var client = await _context.Clients.Include(client => client.User).Include(client => client.City)
                .FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            if (client is null)
                return new BaseResponse<UserData?>() { Code = ErrorCode.BadRequest, Message = "Fail to get user data", Data = null };

            var favLang = client.FavLang;
            var userDataResponse = new UserData(userId,!string.IsNullOrEmpty(client.ProfileImage) ?GetProfileImageUrl(userId, context) : null, client.User.DisplayName, client.User.Email, client.User.PhoneNumber,
                favLang.ToString(),
                new LookupResponse() { Id = client.CityId, Value = favLang == Lang.en? client.City?.NameEn : client.City?.Name }, client is { IsOwner: true }, client.User.IsActive, client.User.PhoneNumberConfirmed);
           
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
            user.NormalizedUserName = newNumber;
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
        public BaseResponse<string?> Encrypt(Domain.Entities.User user)
        {
            var jsonString = JsonConvert.SerializeObject(user);

            using (Aes aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = new byte[16]; // Initialization Vector

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(jsonString);
                        }
                    }

                    var result = Convert.ToBase64String(msEncrypt.ToArray());
                    return new BaseResponse<string?>() {  Code = ErrorCode.Success, Data = result, Message = "User encrypted data"};
                }
            }
        }
        public async Task<BaseResponse<IFileProxy?>> Profile(string userId)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            if (client is null)
                return new BaseResponse<IFileProxy?>() { Code = ErrorCode.BadRequest, Message = "Fail to get user profile pic", Data = null };

            if (string.IsNullOrEmpty(client.ProfileImage))
            {
                var defaultImage = await _fileManager.FileProxy<Domain.Entities.User>("defaultImage.jpg", "");
                var defaultContentType = _fileManager.GetContentType(defaultImage.Filename);
                return new BaseResponse<IFileProxy?>() { Code = ErrorCode.Success, Message = defaultContentType, Data = defaultImage };
            }

			var fileImage = await _fileManager.FileProxy<Domain.Entities.User>(client.ProfileImage, userId);
            var contentType = _fileManager.GetContentType(fileImage.Filename);
            return new BaseResponse<IFileProxy?>() { Code = ErrorCode.Success, Message = contentType, Data = fileImage };
        }
        private string GetProfileImageUrl(string uId,HttpContext context)
        {
            var request = context.Request;

            var scheme = request.Scheme;

            var host = request.Host.Value;

            var url = $"{scheme}://{host}/RentzApi/api/User/Profile?uId={uId}";

            return url;
        }
    }
}

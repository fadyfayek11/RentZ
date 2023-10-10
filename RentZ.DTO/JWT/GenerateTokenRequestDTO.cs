using RentZ.DTO.Enums;

namespace RentZ.DTO.JWT;

public record GenerateTokenRequestDto(string UserId, string UserName, string UserEmail,
	string PhoneNumber, Gender Gender, string Bio, Lang FavLang, string City, bool IsActive, bool IsOtpVerified);

public record GenerateTokenResponseDto(string Token, DateTime ExpiryDate);

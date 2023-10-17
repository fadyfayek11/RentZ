using RentZ.DTO.Enums;

namespace RentZ.DTO.JWT;

public record GenerateTokenRequestDto(string UserId, string DisplayName, string UserEmail,
	string PhoneNumber, Gender Gender, Lang FavLang, bool IsOwner, 
	bool IsActive, bool IsOtpVerified, Roles Role);

public record GenerateTokenResponseDto(string Token, DateTime ExpiryDate);

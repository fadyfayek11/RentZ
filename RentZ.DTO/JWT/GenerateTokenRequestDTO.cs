using RentZ.DTO.Enums;

namespace RentZ.DTO.JWT;

public record GenerateTokenRequestDto(string UserId, string? DisplayName, string? UserEmail, string? UserImage,
	string? PhoneNumber, Lang? FavLang, bool? IsOwner, 
	bool? IsActive, bool? IsOtpVerified, Roles? Role);

public record GenerateTokenResponseDto(string Token, DateTime ExpiryDate);

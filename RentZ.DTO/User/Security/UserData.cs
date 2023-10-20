using RentZ.DTO.Lookups;

namespace RentZ.DTO.User.Security;
public record UserData(string ProfileImage,string DisplayName, string Email, string PhoneNumber, string FavLanguage,
    LookupResponse City, LookupResponse Governorate, string Gender, bool IsOwner, bool IsActive, bool IsOtpVerified);

public record EditUserData(string? DisplayName, string? Email, string? PhoneNumber, int? CityId,  string? Gender);
using RentZ.DTO.Lookups;

namespace RentZ.DTO.User.Security;
public record UserData(string DisplayName, string Email, string PhoneNumber, string FavLanguage,
    LookupResponse City, LookupResponse Governorate, string Gender, bool IsOwner, bool IsActive, bool IsOtpVerified);
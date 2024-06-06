using RentZ.DTO.Enums;
using RentZ.DTO.Lookups;

namespace RentZ.DTO.User.Security;
public record UserData(string UserId, string? ProfileImage,string DisplayName, string Email, string PhoneNumber, string FavLanguage, DateTime BirthDate,
    LookupResponse? City, Gender? Gender, bool IsOwner, bool IsActive, bool IsOtpVerified);

public record AdminUserData(string UserId, string DisplayName, string Email, string PhoneNumber, DateTime BirthDate,
     string Gender,  bool IsActive);

public record SetAdminData(string Email, string PhoneNumber, string Password);
public record UpdateAdminData(string UserId, string? Email, string? PhoneNumber, string? Password, bool? IsActive);
public record AdminData(string UserId, string Email, string PhoneNumber, bool IsActive);

public record EditUserData(string? DisplayName, DateTime? BirthDate, string? Email, int? CityId,  string? Gender);
public record Number(string PhoneNumber);
public record PasswordDto(string Password);
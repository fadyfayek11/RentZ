﻿using RentZ.DTO.Lookups;

namespace RentZ.DTO.User.Security;
public record UserData(string UserId, string? ProfileImage,string DisplayName, string Email, string PhoneNumber, string FavLanguage, DateTime BirthDate,
    LookupResponse? City, LookupResponse? Governorate, string Gender, bool IsOwner, bool IsActive, bool IsOtpVerified);

public record EditUserData(string? DisplayName, DateTime? BirthDate, string? Email, int? CityId,  string? Gender);
public record Number(string PhoneNumber);
public record PasswordDto(string Password);
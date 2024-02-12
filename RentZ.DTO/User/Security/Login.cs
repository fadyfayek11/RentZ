namespace RentZ.DTO.User.Security;

public record Login(string PhoneNumber, string Password);
public record VerifyOtp(string OtpNumber);
public record ChangePhoneNumber(string NewPhoneNumber);
public record ValidateEmail(string Email);
public record ValidateMobile(string Mobile);
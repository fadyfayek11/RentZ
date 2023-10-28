namespace RentZ.DTO.User.Security;

public record Login(string PhoneNumber, string Password);
public record VerifyOtp(string OtpNumber);
public record ChangePhoneNumber(string NewPhoneNumber);
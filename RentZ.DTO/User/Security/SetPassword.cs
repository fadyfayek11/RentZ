namespace RentZ.DTO.User.Security;

public record SetPassword(string UserId, string NewPassword);
public record ChangePassword(string UserId, string OldPassword, string NewPassword);
public record ChangePasswordDto(string OldPassword, string NewPassword);

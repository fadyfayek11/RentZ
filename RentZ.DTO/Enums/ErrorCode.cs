namespace RentZ.DTO.Enums;

public enum ErrorCode
{
    None = 0,
    FailOtp = 1,
    Success = 200,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    ValidationFailed = 422,
    InternalServerError = 500,
}
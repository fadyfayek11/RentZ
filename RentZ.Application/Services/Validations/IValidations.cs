using RentZ.DTO.Response;

namespace RentZ.Application.Services.Validations;

public interface IValidations
{
    Task<BaseResponse<bool>> IsPhoneNumberExist(string phoneNumber);
    Task<BaseResponse<bool>> IsEmailExist(string email);
}
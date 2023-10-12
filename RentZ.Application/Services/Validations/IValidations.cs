using RentZ.DTO.Response;

namespace RentZ.Application.Services.Validations;

public interface IValidations
{
    Task<BaseResponse<bool>> IsUserNameExist(string userName);
    Task<BaseResponse<bool>> IsPhoneNumberExist(string phoneNumber);
    Task<bool> IsCityExist(int cityId);
}
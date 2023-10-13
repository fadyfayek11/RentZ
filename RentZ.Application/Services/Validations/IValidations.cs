namespace RentZ.Application.Services.Validations;

public interface IValidations
{
    Task<bool> IsPhoneNumberExist(string phoneNumber);
    Task<bool> IsCityExist(int cityId);
}
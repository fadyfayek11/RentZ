using FluentValidation;
using RentZ.Application.Services.Validations;
using RentZ.DTO.User.Security;

namespace RentZ.Application.DtoValidations.User;

public class RegistrationValidation : AbstractValidator<Registration>
{
    private readonly IValidations _validations;
    public RegistrationValidation(IValidations validations)
    {
        _validations = validations;

        RuleFor(registration => registration.DisplayName)
            .NotEmpty().WithMessage("Your Display Name is required.");

        RuleFor(registration => registration.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one digit.");


        RuleFor(registration => registration.UserEmail)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

       

        RuleFor(registration => registration.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MustAsync(BeAValidNumber).WithMessage("This phone number registered before.");

        RuleFor(registration => registration.FavLang)
            .IsInEnum().WithMessage("Invalid favorite language.");

    }
    //private async Task<bool> BeAValidCity(int cityId, CancellationToken cancellationToken)
    //{
    //   return  await _validations.IsCityExist(cityId);
    //}
    private async Task<bool> BeAValidNumber(string phoneNumber, CancellationToken cancellationToken)
    {
       var res  =  await _validations.IsPhoneNumberExist(phoneNumber);
       return res.Data;
    }
}
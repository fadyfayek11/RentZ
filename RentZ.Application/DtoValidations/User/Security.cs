﻿using FluentValidation;
using RentZ.Application.Services.Validations;
using RentZ.DTO.User.Security;

namespace RentZ.Application.DtoValidations.User;

public class RegistrationValidation : AbstractValidator<Registration>
{
    private readonly IValidations _validations;
    public RegistrationValidation(IValidations validations)
    {
        _validations = validations;

        RuleFor(registration => registration.UserName)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(registration => registration.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d).*$")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one digit.");


        RuleFor(registration => registration.UserEmail)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(registration => registration.BirthDate)
            .NotEmpty().WithMessage("Birthdate is required.");

        RuleFor(registration => registration.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.");

        RuleFor(registration => registration.Gender)
            .IsInEnum().WithMessage("Invalid gender.");

        RuleFor(registration => registration.FavLang)
            .IsInEnum().WithMessage("Invalid favorite language.");

        RuleFor(registration => registration.CityId)
            .GreaterThan(0).WithMessage("City ID is required.")
            .MustAsync(BeAValidCity).WithMessage("City ID does not exist in the database."); 

        RuleFor(registration => registration.IsOwner)
            .NotNull().WithMessage("IsOwner field is required.");
    }
    private async Task<bool> BeAValidCity(int cityId, CancellationToken cancellationToken)
    {
       return  await _validations.IsCityExist(cityId);
    }
}
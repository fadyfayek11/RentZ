using RentZ.DTO.Enums;
using System;
using FluentValidation;

namespace RentZ.DTO.User.Security;

public record Registration(string UserName, string Password, string UserEmail, DateTime BirthDate,
	string PhoneNumber, string Gender, string FavLang, int CityId, bool IsOwner);

public class RegistrationValidation : AbstractValidator<Registration>
{
	public RegistrationValidation()
	{
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
			.GreaterThan(0).WithMessage("City ID is required.");

		RuleFor(registration => registration.IsOwner)
			.NotNull().WithMessage("IsOwner field is required.");
	}
}
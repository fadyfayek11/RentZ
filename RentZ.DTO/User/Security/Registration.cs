namespace RentZ.DTO.User.Security;

public record Registration(string DisplayName, string Password, string UserEmail, DateTime BirthDate,
	string PhoneNumber, string Gender, string FavLang, int? CityId, bool IsOwner = false);


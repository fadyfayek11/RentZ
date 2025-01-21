namespace RentZ.DTO.User.Security;

public record Registration(string DisplayName, string Password, string UserEmail,
	string PhoneNumber, string FavLang, int? CityId, bool IsOwner = false);


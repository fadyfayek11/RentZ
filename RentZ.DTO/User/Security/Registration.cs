using RentZ.DTO.Enums;

namespace RentZ.DTO.User.Security;

public record Registration(string UserName, string Password, string UserEmail, DateTime BirthDate, string PhoneNumber, Gender Gender, Lang FavLang, int CityId, bool IsOwner);
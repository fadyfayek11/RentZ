#nullable disable
using Microsoft.AspNetCore.Identity;
using RentZ.DTO.Enums;

namespace RentZ.Domain.Entities;

public class User : IdentityUser<Guid>
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public bool IsActive { get; set; } = false;
}
public class Admin : User
{
	public bool IsRoot { get; set; } = false;
}

public class Client : User
{
	public bool IsOwner { get; set; } = false;
    public Gender Gender { get; set; }
    public DateTime BirthDate { get; set; }
    public string ProfileImage { get; set; }
    public string Bio { get; set; }


    public int CityId { get; set; }
    public virtual City City { get; set; }
}
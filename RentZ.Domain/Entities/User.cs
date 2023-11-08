#nullable disable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using RentZ.DTO.Enums;

namespace RentZ.Domain.Entities;

public class User : IdentityUser<Guid>
{
	public string DisplayName { get; set; }
	public bool IsActive { get; set; } = false;
}
public class Admin
{
    [Key]
    public Guid Id { get; set; }
    public bool IsRoot { get; set; } = false;

    public virtual User User { get; set; }

}

public class Client
{
    [Key]
    public Guid Id { get; set; }
    public bool IsOwner { get; set; } = false;
    public Gender Gender { get; set; }
    public DateTime BirthDate { get; set; }
    public string ProfileImage { get; set; }
    public Lang FavLang { get; set; }
    public string Bio { get; set; }
    public int? CityId { get; set; }

    public virtual City? City { get; set; }
    public virtual User User { get; set; }

}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RentZ.DTO.Enums;

namespace RentZ.Domain.Entities;

public class Property
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Client))]
    public Guid OwnerId { get; set; }

    public int CityId { get; set; }
    public string? Name { get; set; } 
    public double Price { get; set; }
    public string? Description { get; set; }
    public int NumOfRooms { get; set; }
    public int NumOfBeds { get; set; }
    public bool Smoking { get; set; }
    public bool Pet { get; set; }
    public bool ForRent { get; set; }
    public bool ForExchange { get; set; }
    public int Views { get; set; }
    public bool Approved { get; set; }

    [ForeignKey(nameof(Admin))]
    public Guid? ApprovedBy { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public PropertyCategory Category { get; set; }
    public FurnishingType FurnishingType { get; set; }
    public virtual ICollection<PropertyUtility>? PropertyUtilities { get; set; }
    public virtual ICollection<Media>? PropertyMedia { get; set; }
    public virtual City City { get; set; } = null!;
    public virtual Client Client { get; set; } = null!;
    public virtual Admin? Admin { get; set; }
}
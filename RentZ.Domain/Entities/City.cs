#nullable disable
namespace RentZ.Domain.Entities;

public class City
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string NameEn { get; set; }

	public int GovernorateId { get; set; }
	public virtual Governorate Governorate { get; set; }
}


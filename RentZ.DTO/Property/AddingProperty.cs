#nullable disable
using Microsoft.AspNetCore.Http;
using RentZ.DTO.Enums;

namespace RentZ.DTO.Property;

public class AddingProperty
{
    public string Name { get; set; }
    public PropertyCategory PropertyCategory { get; set; }
    public PropertyType PropertyType { get; set; }
    public double PriceFrom { get; set; }
    public double PriceTo { get; set; }
    public double Area { get; set; }
    public int CityId { get; set; }
    public string Description { get; set; }
    public int NumOfRooms { get; set; }
    public int NumOfBeds { get; set; }
    public int NumOfBathRooms { get; set; }
    public int NumberOfPeople { get; set; }
    public FurnishingType FurnishingType { get; set; }
    public bool Balcony { get; set; }
    public bool Pet { get; set; }
    public bool ForRent { get; set; }
    public PeriodType PeriodType { get; set; }
    public Gender? Gender { get; set; }
    public int? AgeFrom { get; set; }
    public int? AgeTo { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<int>? PropertyUtilities { get; set; }
    public List<IFormFile> Images { get; set; }
}

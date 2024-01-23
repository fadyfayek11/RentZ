#nullable disable
using RentZ.DTO.Enums;
using RentZ.DTO.Lookups;

namespace RentZ.DTO.Property;

public class GetPropertyDetails
{
    public int Id { get; set; }
    public string Name { get; set; }
    public PropertyCategory Category { get; set; }
    public PropertyType PropertyType { get; set; }
    public double PriceFrom { get; set; }
    public double PriceTo { get; set; }
    public double Area { get; set; }
    public LookupResponse City { get; set; }
    public string Description { get; set; }
    public int NumOfRooms { get; set; }
    public int NumOfBeds { get; set; }
    public int NumOfBathRooms { get; set; }
    public int NumberOfPeople { get; set; }
    public FurnishingType FurnishingType { get; set; }
    public PeriodType PeriodType { get; set; }
    public bool Balcony { get; set; }
    public bool Pet { get; set; }
    public bool ForRent { get; set; }
    public bool IsFav { get; set; }
    public int Views { get; set; }
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
    public DateTime AvailableDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<LookupResponse> PropertyUtilities { get; set; }
    public List<PropMedia> ImagesUrls { get; set; }
    public OwnerDetails Owner { get; set; }
}

public class OwnerDetails
{
    public Guid UId { get; set; }
    public string DisplayName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string ImageUrl { get; set; }
}

public class PropertyFilter
{
    public PropertyType? PropertyType { get; set; }
    public bool IsActive { get; set; } = true;
    public bool? IsApproved { get; set; }
    public Guid? OwnerId { get; set; }
    public int? NumOfRooms { get; set; }
    public double? PriceFrom { get; set; }
    public double? PriceTo{ get; set; }
    public double? Area { get; set; }
    public int? NumOfBeds { get; set; }
    public int? NumOfBathRooms { get; set; }
    public int? CityId { get; set; }
    public PeriodType? PeriodType { get; set; }
    public Gender? Gender { get; set; }
    public int? Age { get; set; }
    public int? NumberOfPeople { get; set; }
    public bool? Balcony { get; set; }
    public bool? Pet { get; set; }
    public bool? ForRent { get; set; }
    public DateTime? AvailableDateFrom { get; set; }
    public DateTime? AvailableDateTo { get; set; }
    public FurnishingType? FurnishingType { get; set; }
    public List<int> PropertyUtilities { get; set; }
    public List<PropertyCategory> PropertyCategories { get; set; }
    public Pagination Pagination { get; set; } = new Pagination();
   
}
public class GuestPropertyFilter
{
    public string Lang { get; set; } = "en";
    public bool IsActive { get; set; } = true;
    public bool? IsApproved { get; set; }
    public int? NumOfRooms { get; set; }
    public double? PriceFrom { get; set; }
    public double? PriceTo{ get; set; }
    public double? Area { get; set; }
    public int? NumOfBeds { get; set; }
    public int? NumOfBathRooms { get; set; }
    public int? CityId { get; set; }
    public PeriodType? PeriodType { get; set; }
    public Gender? Gender { get; set; }
    public int? Age { get; set; }
    public int? NumberOfPeople { get; set; }
    public bool? Balcony { get; set; }
    public bool? Pet { get; set; }
    public DateTime? AvailableDateFrom { get; set; }
    public DateTime? AvailableDateTo { get; set; }
    public FurnishingType? FurnishingType { get; set; }
    public List<int> PropertyUtilities { get; set; }
    public List<PropertyCategory> PropertyCategories { get; set; }
    public Pagination Pagination { get; set; } = new Pagination();
   
}


public class Pagination
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class FindProperty 
{
    public int PropId { get; set; }
    public string Lang { get; set; } = "en";
}

public class PropMedia
{
    public int Id { get; set; }
    public string Url { get; set; }
}

public class PropImage 
{
    public int PropId { get; set; }
    public int ImageId { get; set; }
}

public class PagedResult<T>
{
    public List<GetProperties> Items { get; set; }
    public int TotalCount { get; set; }
}

public class GetProperties
{
    public int Id { get; set; }
    public string Name { get; set; }
    public PropertyCategory Category { get; set; }
    public PropertyType PropertyType { get; set; }
    public PeriodType PeriodType { get; set; }
    public double PriceFrom { get; set; }
    public double PriceTo { get; set; }
    public double Area { get; set; }
    public string Description { get; set; }
    public int NumOfRooms { get; set; }
    public int NumOfBeds { get; set; }
    public int NumOfBathRooms { get; set; }
    public int NumberOfPeople { get; set; }
    public FurnishingType FurnishingType { get; set; }
    public bool ForRent { get; set; }
    public int Views { get; set; }
    public bool IsApproved { get; set; }
    public bool IsFav { get; set; }
    public bool IsActive { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public DateTime CreatedDate { get; set; }
    public LookupResponse City { get; set; }
    public string CoverImageUrl { get; set; }
}
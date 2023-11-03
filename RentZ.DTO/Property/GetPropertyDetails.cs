﻿#nullable disable
using RentZ.DTO.Enums;
using RentZ.DTO.Lookups;

namespace RentZ.DTO.Property;

public class GetPropertyDetails
{
    public string Name { get; set; }
    public PropertyCategory Category { get; set; }
    public double Price { get; set; }
    public double Area { get; set; }
    public LookupResponse City { get; set; }
    public LookupResponse Governorate { get; set; }
    public string Description { get; set; }
    public int NumOfRooms { get; set; }
    public int NumOfBeds { get; set; }
    public int NumOfBathRooms { get; set; }
    public FurnishingType FurnishingType { get; set; }
    public bool Smoking { get; set; }
    public bool Pet { get; set; }
    public bool ForRent { get; set; }
    public bool ForExchange { get; set; }
    public int Views { get; set; }
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
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
}

public class PropertyFilter
{
    public PropertyCategory? Category { get; set; }
    public int? NumOfRooms { get; set; }
    public double? Price { get; set; }
    public double? Area { get; set; }
    public int? NumOfBeds { get; set; }
    public int NumOfBathRooms { get; set; }
    public FurnishingType FurnishingType { get; set; }
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
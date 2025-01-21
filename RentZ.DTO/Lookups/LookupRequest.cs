namespace RentZ.DTO.Lookups;

public record LookupRequest(int Id, string? Name, bool? IsActive, string Lang = "en");
public record AddLookup(string Name,string NameEn, int OrderId);
public record UpdateLookup(int Id,string Name,string NameEn, int OrderId);

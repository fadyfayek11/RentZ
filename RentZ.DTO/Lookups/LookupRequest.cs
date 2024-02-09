namespace RentZ.DTO.Lookups;

public record LookupRequest(int Id, string? Name, string Lang = "en");
public record AddLookup(string Name,string NameEn, int OrderId);

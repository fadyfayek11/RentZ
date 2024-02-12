#nullable disable
namespace RentZ.DTO.Lookups;

public class LookupResponse
{
    public int? Id { get; set; }
    public string Value { get; set; }
}
public class LookupResponseAdmin
{
    public int? Id { get; set; }
    public string Value { get; set; }
    public string ValueEn { get; set; }
}
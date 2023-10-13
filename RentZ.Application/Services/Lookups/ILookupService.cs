using RentZ.DTO.Lookups;

namespace RentZ.Application.Services.Lookups;

public interface ILookupService
{
	Task<List<LookupResponse>> GetCities(int governorateId, LookupRequest lookup);
	Task<List<LookupResponse>> GetGovernorates(LookupRequest lookup);
}
using RentZ.DTO.Lookups;
using RentZ.DTO.Response;

namespace RentZ.Application.Services.Lookups;

public interface ILookupService
{
	Task<BaseResponse<List<LookupResponse>>> GetCities(int governorateId, LookupRequest lookup);
	Task<BaseResponse<List<LookupResponse>>> GetGovernorates(LookupRequest lookup);
}
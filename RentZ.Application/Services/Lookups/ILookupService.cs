using RentZ.DTO.Lookups;
using RentZ.DTO.Response;

namespace RentZ.Application.Services.Lookups;

public interface ILookupService
{
	Task<BaseResponse<List<LookupResponse>>> GetCities(LookupRequest lookup);
	Task<BaseResponse<bool>> AddCity(AddLookup lookup);
	Task<BaseResponse<List<LookupResponse>>> GetPropertyUtilities(LookupRequest lookup);
	Task<BaseResponse<bool>> AddPropertyUtilities(AddLookup lookup);
}
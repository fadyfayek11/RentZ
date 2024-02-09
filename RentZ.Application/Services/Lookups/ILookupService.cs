using RentZ.DTO.Lookups;
using RentZ.DTO.Response;

namespace RentZ.Application.Services.Lookups;

public interface ILookupService
{
	Task<BaseResponse<List<LookupResponse>>> GetCities(LookupRequest lookup);
	Task<BaseResponse<bool>> AddCity(int governorateId, LookupRequest lookup);
	Task<BaseResponse<List<LookupResponse>>> GetPropertyUtilities(LookupRequest lookup);
	Task<BaseResponse<bool>> AddPropertyUtilities(LookupRequest lookup);
}
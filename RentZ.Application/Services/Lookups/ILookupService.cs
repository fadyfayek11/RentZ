using RentZ.DTO.Lookups;
using RentZ.DTO.Response;

namespace RentZ.Application.Services.Lookups;

public interface ILookupService
{
	Task<BaseResponse<List<LookupResponse>>> GetCities(LookupRequest lookup);
    Task<BaseResponse<List<LookupResponseAdmin>>> GetAdminCities(LookupRequest lookup);

    Task<BaseResponse<bool>> AddCity(AddLookup lookup);
	Task<BaseResponse<bool>> CityActivation(int lookupId);

	Task<BaseResponse<List<LookupResponse>>> GetUtilities(LookupRequest lookup);
	Task<BaseResponse<List<LookupResponseAdmin>>> GetAdminUtilities(LookupRequest lookup);
	
    Task<BaseResponse<bool>> AddUtility(AddLookup lookup);
	Task<BaseResponse<bool>> UtilityActivation(int lookupId);
}
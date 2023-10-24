using Microsoft.EntityFrameworkCore;
using RentZ.DTO.Enums;
using RentZ.DTO.Lookups;
using RentZ.DTO.Response;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.Lookups;

public class LookupService : ILookupService
{
	private readonly ApplicationDbContext _context;
    public LookupService(ApplicationDbContext context)
    {
	    _context = context;
    }

    public async Task<BaseResponse<List<LookupResponse>>> GetCities(int governorateId, LookupRequest lookup)
    {
		var cityQuery = _context.City.AsQueryable(); 

		if (governorateId != 0)
		{
			cityQuery = cityQuery.Where(x => x.GovernorateId == governorateId);
		}

		if (lookup.Id != 0)
		{
			cityQuery = cityQuery.Where(x => x.Id == lookup.Id);
		}

		if (!string.IsNullOrEmpty(lookup.Name))
		{
			cityQuery = cityQuery.Where(x => x.Name.Contains(lookup.Name) || x.NameEn.Contains(lookup.Name));
		}

        var isEnum = Enum.TryParse(lookup.Lang, out Lang langValue);

        var cities = await cityQuery
			.Select(x => new LookupResponse
			{
				Id = x.Id,
				Value = isEnum && langValue == Lang.en ? x.NameEn : x.Name,
            })
        .ToListAsync();

        return new BaseResponse<List<LookupResponse>>() { Code = ErrorCode.Success, Data = cities, Errors = null, Message = "Get all cities" };
    }

    public async Task<BaseResponse<List<LookupResponse>>> GetGovernorates(LookupRequest lookup)
	{
		var governorateQuery = _context.Governorate.AsQueryable();

		if (lookup.Id != 0)
		{
			governorateQuery = governorateQuery.Where(x => x.Id == lookup.Id);
		}

		if (!string.IsNullOrEmpty(lookup.Name))
		{
			governorateQuery = governorateQuery.Where(x => x.Name.Contains(lookup.Name) || x.NameEn.Contains(lookup.Name));
		}

        var isEnum = Enum.TryParse(lookup.Lang, out Lang langValue);
        
        var governorate = await governorateQuery
			.Select(x => new LookupResponse
			{
				Id = x.Id,
				Value = isEnum && langValue  == Lang.en ? x.NameEn : x.Name,
			})
			.ToListAsync();

		return new BaseResponse<List<LookupResponse>>(){Code = ErrorCode.Success, Data = governorate, Errors = null, Message = "Get all governorates" };
	}
}
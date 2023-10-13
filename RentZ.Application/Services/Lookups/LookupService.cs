using Microsoft.EntityFrameworkCore;
using RentZ.DTO.Enums;
using RentZ.DTO.Lookups;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.Lookups;

public class LookupService : ILookupService
{
	private readonly ApplicationDbContext _context;
    public LookupService(ApplicationDbContext context)
    {
	    _context = context;
    }

    public async Task<List<LookupResponse>> GetCities(int governorateId, LookupRequest lookup)
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

		var cities = await cityQuery
			.Select(x => new LookupResponse
			{
				Id = x.Id,
				Value = (Lang)Enum.Parse(typeof(Lang), lookup.Lang) == Lang.En ? x.NameEn : x.Name,
			})
			.ToListAsync();

		return cities;
    }

	public async Task<List<LookupResponse>> GetGovernorates(LookupRequest lookup)
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

		var governorate = await governorateQuery
			.Select(x => new LookupResponse
			{
				Id = x.Id,
				Value = (Lang)Enum.Parse(typeof(Lang), lookup.Lang) == Lang.En ? x.NameEn : x.Name,
			})
			.ToListAsync();

		return governorate;
	}
}
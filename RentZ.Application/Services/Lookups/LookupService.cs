using Microsoft.EntityFrameworkCore;
using RentZ.Domain.Entities;
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

    public async Task<BaseResponse<List<LookupResponse>>> GetCities(LookupRequest lookup)
    {
		var cityQuery = _context.City.AsQueryable();

        if (lookup.Id != 0)
		{
			cityQuery = cityQuery.Where(x => x.Id == lookup.Id);
		}

		if (!string.IsNullOrEmpty(lookup.Name))
		{
			cityQuery = cityQuery.Where(x => x.Name.Contains(lookup.Name) || x.NameEn.Contains(lookup.Name)).OrderBy(x => x.ViewOrder);
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

    public async Task<BaseResponse<bool>> AddCity(AddLookup lookup)
    {
        await _context.City.AddAsync(new City
        {
            Name = lookup.Name,
            NameEn = lookup.NameEn,
            ViewOrder = lookup.OrderId,
            IsActive = true
        });

        await _context.SaveChangesAsync();
        return new BaseResponse<bool>() { Code = ErrorCode.Success, Data = true, Errors = null, Message = "Add new city" };
    }

    public async Task<BaseResponse<bool>> CityActivation(int lookupId)
    {
        var city = await _context.City.FirstOrDefaultAsync(x => x.Id == lookupId);
        if (city is null)
            return new BaseResponse<bool>() { Code = ErrorCode.BadRequest, Message = "Fail to city data", Data = false };

        city.IsActive = !city.IsActive;
        _context.City.Update(city);
        await _context.SaveChangesAsync();

        return new BaseResponse<bool>() { Code = ErrorCode.Success, Message = city.IsActive ? "Activate city done" : "Deactivate city done", Data = city.IsActive };
    }

    public async Task<BaseResponse<List<LookupResponse>>> GetPropertyUtilities(LookupRequest lookup)
    {
        var utilityQuery = _context.Utilities.AsQueryable();

        if (lookup.Id != 0)
        {
            utilityQuery = utilityQuery.Where(x => x.Id == lookup.Id);
        }

        if (!string.IsNullOrEmpty(lookup.Name))
        {
            utilityQuery = utilityQuery.Where(x => x.Name.Contains(lookup.Name) || x.NameEn.Contains(lookup.Name));
        }

        var isEnum = Enum.TryParse(lookup.Lang, out Lang langValue);

        var utilities = await utilityQuery
            .Select(x => new LookupResponse
            {
                Id = x.Id,
                Value = isEnum && langValue == Lang.en ? x.NameEn : x.Name,
            })
            .ToListAsync();

        return new BaseResponse<List<LookupResponse>>() { Code = ErrorCode.Success, Data = utilities, Errors = null, Message = "Get all governorates" };
    }

    public async Task<BaseResponse<bool>> AddUtility(AddLookup lookup)
    {
        await _context.Utilities.AddAsync(new Utility()
        {
            Name = lookup.Name,
            NameEn = lookup.NameEn,
            IsActive = true
        });

        await _context.SaveChangesAsync();
        return new BaseResponse<bool>() { Code = ErrorCode.Success, Data = true, Errors = null, Message = "Add new Utility" };
    }

    public async Task<BaseResponse<bool>> UtilityActivation(int lookupId)
    {
        var utility = await _context.Utilities.FirstOrDefaultAsync(x => x.Id == lookupId);
        if (utility is null)
            return new BaseResponse<bool>() { Code = ErrorCode.BadRequest, Message = "Fail to get utility data", Data = false };

        utility.IsActive = !utility.IsActive;
        _context.Utilities.Update(utility);
        await _context.SaveChangesAsync();

        return new BaseResponse<bool>() { Code = ErrorCode.Success, Message = utility.IsActive ? "Activate utility done" : "Deactivate utility done", Data = utility.IsActive };
    }
}
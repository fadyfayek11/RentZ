using Microsoft.AspNetCore.Http;
using RentZ.Domain.Entities;
using RentZ.DTO.Enums;
using RentZ.DTO.Property;
using RentZ.DTO.Response;
using RentZ.Infrastructure.Context;
using System.Security.Claims;
using ExtCore.FileStorage.Abstractions;
using Microsoft.EntityFrameworkCore;
using RentZ.Application.Mapper;
using RentZ.Application.Services.Files;
using RentZ.DTO.Lookups;

namespace RentZ.Application.Services.Property;

public class PropertyService : IPropertyService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileManager _fileManager;

    public PropertyService(ApplicationDbContext context, IFileManager fileManager)
    {
        _context = context;
        _fileManager = fileManager;
    }

    public async Task<BaseResponse<int>> ViewProperty(FindProperty filter)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(x => x.Id == filter.PropId);

        if (property is null)
            return new BaseResponse<int>() { Code = ErrorCode.BadRequest, Message = "Fail to find the property", Data = 0 };

        property.Views++;

        _context.Properties.Update(property);
        await _context.SaveChangesAsync();
        return new BaseResponse<int>() { Code = ErrorCode.Success, Message = "New view happened", Data = property.Views };
    }

    public async Task<BaseResponse<GetPropertyDetails?>> AddProperty(HttpContext context, AddingProperty prop)
    {
        var userId = context.User.FindFirstValue("UserId");

        var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
        if (client is null)
            return new BaseResponse<GetPropertyDetails?>() { Code = ErrorCode.BadRequest, Message = "Fail to post a property", Data = null };

        var propEntity = Mapping.Mapper.Map<Domain.Entities.Property>(prop);
        propEntity.OwnerId = Guid.Parse(userId);
        var propUtilities = prop.PropertyUtilities?.Select(x => new PropertyUtility
        {
            UtilityId = x,
        }).ToList();
        propEntity.PropertyUtilities = propUtilities;
        propEntity.Status = prop.PropertyType == PropertyType.Request ? PropertyStatus.Approved : PropertyStatus.Pending;

        await _context.Properties.AddAsync(propEntity);
        await _context.SaveChangesAsync();

        propEntity.PropertyMedia = await AddMedia(prop.Images, propEntity.Id.ToString());
        _context.Update(propEntity);
        await _context.SaveChangesAsync();

        return await GetProperty(context,new FindProperty(){PropId = propEntity.Id,Lang = propEntity.Client.FavLang.ToString()});
    }

    public async Task<BaseResponse<bool>> DeleteProperty(string uId, FindProperty filter)
    {

        var property = await _context.Properties.FirstOrDefaultAsync(x => x.Id == filter.PropId);

        if (property is null)
            return new BaseResponse<bool>() { Code = ErrorCode.BadRequest, Message = "Fail to find the property", Data = false };
        
        if (property.OwnerId != Guid.Parse(uId))
            return new BaseResponse<bool>() { Code = ErrorCode.Unauthorized, Message = "Fail to find the property", Data = false };

        property.IsActive = false;
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();

        return new BaseResponse<bool>() { Code = ErrorCode.Success, Message = "Deleting the property done successfully", Data = true };
    }

    public async Task<BaseResponse<GetPropertyDetails?>> GetProperty(HttpContext context, FindProperty filters)
    {
        var property = await _context.Properties.Include(property => property.Client).ThenInclude(client => client.User)
            .Include(property => property.PropertyMedia).Include(property => property.PropertyUtilities)!
            .ThenInclude(propertyUtility => propertyUtility.Utility).Include(property => property.City)
            .ThenInclude(city => city.Governorate).Include(property => property.FavProperties)
            .FirstOrDefaultAsync(x=>x.Id == filters.PropId);

        if(property is null)
            return new BaseResponse<GetPropertyDetails?>() { Code = ErrorCode.BadRequest, Message = "Fail to find the property", Data = null };

        var propDetails = Mapping.Mapper.Map<GetPropertyDetails>(property);
        var isEnum = Enum.TryParse(filters.Lang, out Lang langValue);

        var userId = context.User.FindFirstValue("UserId") ?? "";
        var isLogInUser = !string.IsNullOrEmpty(userId);

        propDetails.IsFav = isLogInUser && (bool)property.FavProperties?.Where(y => y.ClientId == Guid.Parse(userId) && y.PropertyId == property.Id).Select(x=>x.IsActive).FirstOrDefault();
        propDetails.PropertyUtilities = property.PropertyUtilities?.Select(x=> new LookupResponse
        {
            Id = x.UtilityId,
            Value = isEnum && langValue == Lang.en ? x.Utility.NameEn : x.Utility.Name,
        }).ToList();
        propDetails.City = new LookupResponse
        {
            Id = property.CityId,
            Value = isEnum && langValue == Lang.en ? property.City.NameEn : property.City.Name,
        };
       
        propDetails.ImagesUrls = property.PropertyMedia?.Select(x => new PropMedia
        {
            Id = x.Id,
            Url = GetImageUrl(property.Id.ToString(), x.Id.ToString(), context)
        }).ToList();
        propDetails.Owner = new OwnerDetails
        {
            UId = property.Client.Id,
            DisplayName = property.Client.User.DisplayName,
            PhoneNumber = property.Client.User.PhoneNumber,
            Email = property.Client.User.Email,
            ImageUrl = GetUserImageUrl(property.Client.Id.ToString(), context)
        };

        return new BaseResponse<GetPropertyDetails?>() { Code = ErrorCode.Success, Message = "Get the property details done successfully", Data = propDetails };
    }

    public async Task<BaseResponse<PagedResult<GetProperties?>>> GetProperties(HttpContext context, PropertyFilter filters)
    {
        var userId = context.User.FindFirstValue("UserId") ?? "";
        var client = await _context.Clients.Where(x => x.Id == Guid.Parse(userId)).FirstOrDefaultAsync();

        if (filters.PropertyType == PropertyType.Exchange)
        {
            var clientHasProp = client?.Properties?.FirstOrDefault(x => (x.PropertyType is PropertyType.Exchange or PropertyType.Advertising) && x.IsActive);
            if (clientHasProp is null)
            {
                return new BaseResponse<PagedResult<GetProperties?>> { Code = ErrorCode.BadRequest, Message = "User hasn't any property yet", Data = new PagedResult<GetProperties?>() { Items = new List<GetProperties>(), TotalCount = 0 } };
            }
        }


        var properties = _context.Properties.AsQueryable();

        properties = properties.Where(p => p.IsActive == filters.IsActive && 
            (!filters.PropertyType.HasValue || p.PropertyType == filters.PropertyType) &&
            (!filters.NumberOfPeople.HasValue || p.NumberOfPeople == filters.NumberOfPeople) &&
            (!filters.ForRent.HasValue || p.ForRent == filters.ForRent) &&
            (!filters.CityId.HasValue || p.CityId == filters.CityId) &&
            (!filters.OwnerId.HasValue || p.OwnerId == filters.OwnerId) &&
            (!filters.Status.HasValue || p.Status == filters.Status) &&
            (!filters.Pet.HasValue || p.Pet == filters.Pet) &&
            (!filters.Balcony.HasValue || p.Balcony == filters.Balcony) &&
            (!filters.PeriodType.HasValue || p.PeriodType == filters.PeriodType) &&
            (!filters.Gender.HasValue || p.Gender == filters.Gender) &&
            (!filters.Age.HasValue || (p.AgeFrom <= filters.Age && p.AgeTo >= filters.Age)) &&
            (!filters.NumOfRooms.HasValue || p.NumOfRooms == filters.NumOfRooms) &&
            (!filters.PriceFrom.HasValue || !filters.PriceTo.HasValue || (p.PriceFrom >= filters.PriceFrom && p.PriceTo <= filters.PriceTo)) &&
            (!filters.Area.HasValue || p.Area <= filters.Area) &&
            (!filters.AvailableDateFrom.HasValue || !filters.AvailableDateTo.HasValue || (p.DateTo <= filters.AvailableDateTo && p.DateFrom >= filters.AvailableDateFrom)) &&
            (!filters.NumOfBeds.HasValue || p.NumOfBeds == filters.NumOfBeds) &&
            (!filters.NumOfBathRooms.HasValue || p.NumOfBathRooms == filters.NumOfBathRooms) &&
            (!filters.FurnishingType.HasValue || p.FurnishingType == filters.FurnishingType)
        );

        if (filters.PropertyType is null)
        {
            properties = properties.Where(property => property.PropertyType == PropertyType.Advertising || property.PropertyType == PropertyType.Exchange);
        }

        if (filters.PropertyUtilities is { Count: > 0 })
        {
            properties = properties.Where(property => property.PropertyUtilities != null && property.PropertyUtilities.Any(util => filters.PropertyUtilities.Contains(util.PropertyId))
            );
        }
        
        if (filters.PropertyCategories is { Count: > 0 })
        {
            properties = properties.Where(property =>  filters.PropertyCategories.Contains(property.PropertyCategory));
        }
        
       

        var propertiesList = await properties.Skip((filters.Pagination.PageIndex-1) * filters.Pagination.PageSize).Take(filters.Pagination.PageSize).OrderByDescending(x => x.CreatedDate).ToListAsync();
        var propertiesResult = Mapping.Mapper.Map<List<GetProperties>>(propertiesList);


        propertiesResult = propertiesResult.Select(x =>
        {
            var coverId = propertiesList.FirstOrDefault(c => c.Id == x.Id)?.PropertyMedia?.FirstOrDefault()?.Id;

            x.CoverImageUrl = coverId is not null && coverId != 0 ? GetImageUrl(x.Id.ToString(),coverId.ToString()!, context) : string.Empty;
            
            x.IsFav = (bool) propertiesList.Select(z => z.FavProperties?
                    .Where(y => y.ClientId == Guid.Parse(userId) && y.PropertyId == x.Id)
                    .Select(c=> c.IsActive).FirstOrDefault())
                    .FirstOrDefault();

            x.City = propertiesList?.Where(c => c.Id == x.Id).Select(c => new LookupResponse()
            {
                Id = c.CityId,
                Value = client?.FavLang == Lang.ar ? c.City.Name : c.City.NameEn,
            }).FirstOrDefault();
            return x;
        }).ToList();

        return new BaseResponse<PagedResult<GetProperties?>> { Code = ErrorCode.Success, Message = "Get the property details done successfully", Data = new PagedResult<GetProperties?>(){Items = propertiesResult, TotalCount = properties.Count()} };
    }
    
    public async Task<BaseResponse<PagedResult<GetProperties?>>> GetGuestProperties(HttpContext context, GuestPropertyFilter filters)
    {
        var properties = _context.Properties.AsQueryable();

        properties = properties.Where(p => p.IsActive == filters.IsActive && p.PropertyType == PropertyType.Advertising &&
            (!filters.CityId.HasValue || p.CityId == filters.CityId) &&
            (!filters.NumberOfPeople.HasValue || p.NumberOfPeople == filters.NumberOfPeople) &&
            (!filters.Status.HasValue || p.Status == filters.Status) &&
            (!filters.Pet.HasValue || p.Pet == filters.Pet) &&
            (!filters.Balcony.HasValue || p.Balcony == filters.Balcony) &&
            (!filters.PeriodType.HasValue || p.PeriodType == filters.PeriodType) &&
            (!filters.Gender.HasValue || p.Gender == filters.Gender) &&
            (!filters.Age.HasValue || (p.AgeFrom <= filters.Age && p.AgeTo >= filters.Age)) &&
            (!filters.NumOfRooms.HasValue || p.NumOfRooms == filters.NumOfRooms) &&
            (!filters.PriceFrom.HasValue || !filters.PriceTo.HasValue || (p.PriceFrom >= filters.PriceFrom && p.PriceTo <= filters.PriceTo)) &&
            (!filters.Area.HasValue || p.Area <= filters.Area) &&
            (!filters.AvailableDateFrom.HasValue || !filters.AvailableDateTo.HasValue || (p.DateTo <= filters.AvailableDateTo && p.DateFrom >= filters.AvailableDateFrom)) &&
            (!filters.NumOfBeds.HasValue || p.NumOfBeds == filters.NumOfBeds) &&
            (!filters.NumOfBathRooms.HasValue || p.NumOfBathRooms == filters.NumOfBathRooms) &&
            (!filters.FurnishingType.HasValue || p.FurnishingType == filters.FurnishingType)
        );

        if (filters.PropertyUtilities is { Count: > 0 })
        {
            properties = properties.Where(property => property.PropertyUtilities != null && property.PropertyUtilities.Any(util => filters.PropertyUtilities.Contains(util.PropertyId))
            );
        }
        
        if (filters.PropertyCategories is { Count: > 0 })
        {
            properties = properties.Where(property =>  filters.PropertyCategories.Contains(property.PropertyCategory));
        }

        var propertiesList = await properties.Skip((filters.Pagination.PageIndex-1) * filters.Pagination.PageSize).Take(filters.Pagination.PageSize).OrderByDescending(x => x.CreatedDate).Include(x=> x.City).ToListAsync();

        var propertiesResult = Mapping.Mapper.Map<List<GetProperties>>(propertiesList);

        propertiesResult = propertiesResult.Select(x =>
        {
            var coverId = propertiesList?.FirstOrDefault(c => c.Id == x.Id)?.PropertyMedia?.FirstOrDefault()?.Id;

            x.CoverImageUrl = coverId is not null && coverId != 0 ? GetImageUrl(x.Id.ToString(),coverId.ToString()!, context) : string.Empty;
            x.City = propertiesList?.Where(c => c.Id == x.Id).Select(c => new LookupResponse()
            {
                Id = c.CityId,
                Value = filters.Lang == Lang.ar.ToString() ? c.City.Name : c.City.NameEn,
            }).FirstOrDefault();
            return x;
        }).ToList();

        return new BaseResponse<PagedResult<GetProperties?>> { Code = ErrorCode.Success, Message = "Get the property details done successfully", Data = new PagedResult<GetProperties?>(){Items = propertiesResult, TotalCount = properties.Count()} };
    }

    public async Task<BaseResponse<IFileProxy?>> PropertyImage(PropImage image)
    {
        var property = await _context.Properties.Include(property => property.PropertyMedia).FirstOrDefaultAsync(x => x.Id == image.PropId);
        if (property is null)
            return new BaseResponse<IFileProxy?>() { Code = ErrorCode.BadRequest, Message = "Fail to get property pic", Data = null };

        var imageRef = property.PropertyMedia?.Where(x => x.Id == image.ImageId).Select(x => x.Reference).FirstOrDefault();
        if (string.IsNullOrEmpty(imageRef))
            return new BaseResponse<IFileProxy?>() { Code = ErrorCode.InternalServerError, Message = "Property hadn't set a pic yet", Data = null };

        var fileImage = await _fileManager.FileProxy<Domain.Entities.Property>(imageRef, image.PropId.ToString());
        var contentType = _fileManager.GetContentType(fileImage.Filename);
        return new BaseResponse<IFileProxy?>() { Code = ErrorCode.Success, Message = contentType, Data = fileImage };
    }

    public async Task<BaseResponse<bool>> FavoriteProperty(string uId, int propId)
    {
        var property = await _context.FavProperties.FirstOrDefaultAsync(x => x.ClientId == Guid.Parse(uId) && x.PropertyId == propId);

        if (property is null)
        {
            await _context.FavProperties.AddAsync(new FavProperty()
            {
                ClientId = Guid.Parse(uId),
                IsActive = true,
                PropertyId = propId
            });
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>()
                { Code = ErrorCode.Success, Message = "Adding property to fav", Data = true };
        }

        property.IsActive = !property.IsActive;

        _context.FavProperties.Update(property);
        await _context.SaveChangesAsync();

        return new BaseResponse<bool>()
            { Code = ErrorCode.Success, Message = property.IsActive ? "Adding property to fav" : "Removing then property from user favorite", Data = property.IsActive };
    }

    public async Task<BaseResponse<PagedResult<GetProperties?>?>> GetUserFavoriteProperties(string uId, HttpContext context, Pagination pagination)
    {
        var client = await _context.Clients.Include(client => client.FavProperties)
            .ThenInclude(favProperty => favProperty.Property).FirstOrDefaultAsync(x => x.Id == Guid.Parse(uId));

        if (client is null)
            return new BaseResponse<PagedResult<GetProperties?>?>()
                { Code = ErrorCode.Unauthorized, Message = "Un-Auth user", Data = null };


        var propertiesList = client.FavProperties?.Where(x=>x.IsActive).Select(x => x.Property).Skip((pagination.PageIndex - 1) * pagination.PageSize).Take(pagination.PageSize).OrderByDescending(x => x.CreatedDate).ToList();
        var totalCount = client.FavProperties?.Count();

        var propertiesResult = Mapping.Mapper.Map<List<GetProperties>>(propertiesList);

        propertiesResult = propertiesResult.Select(x =>
        {
            var coverId = propertiesList?.FirstOrDefault(c => c.Id == x.Id)?.PropertyMedia?.FirstOrDefault()?.Id;

            x.CoverImageUrl = coverId is not null && coverId != 0 ? GetImageUrl(x.Id.ToString(), coverId.ToString()!, context) : string.Empty;
            x.IsFav = true;

            x.City = propertiesList?.Where(c => c.Id == x.Id).Select(c => new LookupResponse()
            {
                Id = c.CityId,
                Value = client?.FavLang == Lang.ar ? c.City.Name : c.City.NameEn,
            }).FirstOrDefault();
            return x;
        }).ToList();

        return new BaseResponse<PagedResult<GetProperties?>?> { Code = ErrorCode.Success, Message = "Get the property details done successfully", Data = new PagedResult<GetProperties?>() { Items = propertiesResult, TotalCount = totalCount ?? 0} };
    }

    private string GetImageUrl(string propId,string imageId, HttpContext context)
    {
        var request = context.Request;

        var scheme = request.Scheme;

        var host = request.Host.Value;

        var url = $"{scheme}://{host}/api/Property/Image?PropId={propId}&ImageId={imageId}";

        return url;
    }
    private string GetUserImageUrl(string uId, HttpContext context)
    {
        var request = context.Request;

        var scheme = request.Scheme;

        var host = request.Host.Value;

        var url = $"{scheme}://{host}/api/User/Profile?uId={uId}";

        return url;
    }
    private async Task<List<Media>> AddMedia(List<IFormFile>? images, string propId)
    {
        var result = new List<Media>();
        if (images != null)
            foreach (var image in images)
            {
                var fileName = $"{Guid.NewGuid()}-{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(image.FileName)}";
                await _fileManager.SaveFileAsync<Domain.Entities.Property>(image, fileName, propId);

                result.Add(new Media() { Reference = fileName });
            }

        return result;
    }

}
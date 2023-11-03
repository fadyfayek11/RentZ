using Microsoft.AspNetCore.Http;
using RentZ.Domain.Entities;
using RentZ.DTO.Enums;
using RentZ.DTO.Property;
using RentZ.DTO.Response;
using RentZ.Infrastructure.Context;
using System.Security.Claims;
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
    public async Task<BaseResponse<int>> AddProperty(HttpContext context, AddingProperty prop)
    {
        var userId = context.User.FindFirstValue("UserId");

        var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
        if (client is null)
            return new BaseResponse<int>() { Code = ErrorCode.BadRequest, Message = "Fail to post a property", Data = 0 };

        var propEntity = Mapping.Mapper.Map<Domain.Entities.Property>(prop);
        propEntity.OwnerId = Guid.Parse(userId);
        var propUtilities = prop.PropertyUtilities.Select(x => new PropertyUtility
        {
            UtilityId = x,
        }).ToList();
        propEntity.PropertyUtilities = propUtilities;

        await _context.Properties.AddAsync(propEntity);
        await _context.SaveChangesAsync();

        propEntity.PropertyMedia = await AddMedia(prop.Images, propEntity.Id.ToString());
        _context.Update(propEntity);
        await _context.SaveChangesAsync();

        return new BaseResponse<int>() { Code = ErrorCode.Success, Message = "Post a property done successfully", Data = propEntity.Id };
    }

    public async Task<BaseResponse<GetPropertyDetails?>> GetProperty(FindProperty filters)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(x=>x.Id == filters.PropId);

        if(property is null)
            return new BaseResponse<GetPropertyDetails?>() { Code = ErrorCode.BadRequest, Message = "Fail to find the property", Data = null };

        var propDetails = Mapping.Mapper.Map<GetPropertyDetails>(property);
        var isEnum = Enum.TryParse(filters.Lang, out Lang langValue);

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
        propDetails.Governorate = new LookupResponse
        {
            Id = property.City.GovernorateId,
            Value = isEnum && langValue == Lang.en ? property.City.Governorate.NameEn : property.City.Governorate.Name,
        };
        propDetails.ImagesUrls = property.PropertyMedia?.Select(x => new PropMedia
        {
            Url = x.Reference
        }).ToList();
        propDetails.Owner = new OwnerDetails
        {
            UId = property.Client.Id,
            DisplayName = property.Client.User.DisplayName,
            PhoneNumber = property.Client.User.PhoneNumber,
            Email = property.Client.User.Email,
        };


        return new BaseResponse<GetPropertyDetails?>() { Code = ErrorCode.Success, Message = "Get the property details done successfully", Data = propDetails };
    }
    private async Task<List<Media>> AddMedia(List<IFormFile> images, string propId)
    {
        var result = new List<Media>();
        foreach (var image in images)
        {
            var fileName = $"{Guid.NewGuid()}-{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(image.FileName)}";
            await _fileManager.SaveFileAsync<Domain.Entities.Property>(image, fileName, propId);
            
            result.Add(new Media(){Reference = fileName});
        }
        return result;
    }
}
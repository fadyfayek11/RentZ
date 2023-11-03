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

    private async Task<List<Media>> AddMedia(List<IFormFile> images, string propId)
    {
        var result = new List<Media>();
        foreach (var image in images)
        {
            var fileName = $"{Guid.NewGuid()}-{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(image.FileName)}";
            await _fileManager.SaveFileAsync<Domain.Entities.User>(image, fileName, propId);
            
            result.Add(new Media(){Reference = fileName});
        }
        return result;
    }
}
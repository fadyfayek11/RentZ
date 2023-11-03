using ExtCore.FileStorage.Abstractions;
using Microsoft.AspNetCore.Http;
using RentZ.DTO.Property;
using RentZ.DTO.Response;

namespace RentZ.Application.Services.Property;

public interface IPropertyService
{
    Task<BaseResponse<int>> AddProperty(HttpContext context, AddingProperty prop);
    Task<BaseResponse<GetPropertyDetails?>> GetProperty(HttpContext context, FindProperty filters);
    Task<BaseResponse<IFileProxy?>> PropertyImage(PropImage image);

}
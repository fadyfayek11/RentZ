using ExtCore.FileStorage.Abstractions;
using Microsoft.AspNetCore.Http;
using RentZ.DTO.Property;
using RentZ.DTO.Response;

namespace RentZ.Application.Services.Property;

public interface IPropertyService
{
    Task<BaseResponse<int>> ViewProperty(FindProperty filter);
    Task<BaseResponse<GetPropertyDetails?>> AddProperty(HttpContext context, AddingProperty prop);
    Task<BaseResponse<bool>> DeleteProperty(string uId, FindProperty filter);
    Task<BaseResponse<GetPropertyDetails?>> GetProperty(HttpContext context, FindProperty filters);
    Task<BaseResponse<PagedResult<GetProperties?>>> GetProperties(HttpContext context, PropertyFilter filters);
    Task<BaseResponse<IFileProxy?>> PropertyImage(PropImage image);

}
using Microsoft.AspNetCore.Http;
using RentZ.DTO.Property;
using RentZ.DTO.Response;

namespace RentZ.Application.Services.Property;

public interface IPropertyService
{
    Task<BaseResponse<int>> AddProperty(HttpContext context, AddingProperty prop);
}
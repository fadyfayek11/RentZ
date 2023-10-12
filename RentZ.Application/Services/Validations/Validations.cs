using Microsoft.EntityFrameworkCore;
using RentZ.DTO.Enums;
using RentZ.DTO.Response;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.Validations;

public class Validations : IValidations
{
    private readonly ApplicationDbContext _context;

    public Validations(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<BaseResponse<bool>> IsUserNameExist(string userName)
    {
        var isExist = await _context.Users.AnyAsync(x => x.UserName == userName);
        return new BaseResponse<bool>()
        {
            Code = isExist ? ErrorCode.BadRequest : ErrorCode.Success, Data = isExist,
            Message = isExist ? "Username already exist, please try another one" : "Valid username"
        };
    }

    public async Task<BaseResponse<bool>> IsPhoneNumberExist(string phoneNumber)
    {
        var isExist = await _context.Users.AnyAsync(x => x.PhoneNumber == phoneNumber);
        return new BaseResponse<bool>()
        {
            Code = isExist ? ErrorCode.BadRequest : ErrorCode.Success, Data = isExist,
            Message = isExist ? "Phone number already exist, please try another one" : "Valid phone number"
        };
    }

    public async Task<bool> IsCityExist(int cityId)
    {
        return await _context.City.AnyAsync(x => x.Id == cityId);
    }
}

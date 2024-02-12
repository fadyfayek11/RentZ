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

    public async Task<BaseResponse<bool>> IsPhoneNumberExist(string phoneNumber)
    {
	    var isExistMobile =  await _context.Users.AnyAsync(x => x.PhoneNumber == phoneNumber);
        
        return new BaseResponse<bool>
        {
            Code = ErrorCode.Success,
            Message = $"Is mobile exist {isExistMobile}",
            Data = isExistMobile,
            Errors = null
        };
    }

    public async Task<BaseResponse<bool>> IsEmailExist(string email)
    {
        var isExistEmail = await _context.Users.AnyAsync(x => x.Email == email);

        return new BaseResponse<bool>
        {
            Code = ErrorCode.Success,
            Message = $"Is email exist {isExistEmail}",
            Data = isExistEmail,
            Errors = null
        };
    }

}

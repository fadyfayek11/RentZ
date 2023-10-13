using Microsoft.EntityFrameworkCore;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.Validations;

public class Validations : IValidations
{
    private readonly ApplicationDbContext _context;

    public Validations(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsPhoneNumberExist(string phoneNumber)
    {
	    return await _context.Users.AnyAsync(x => x.PhoneNumber == phoneNumber);
    }

    public async Task<bool> IsCityExist(int cityId)
    {
        return await _context.City.AnyAsync(x => x.Id == cityId);
    }
}

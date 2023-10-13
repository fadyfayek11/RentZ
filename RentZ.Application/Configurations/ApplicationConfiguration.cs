using Microsoft.Extensions.DependencyInjection;
using RentZ.Application.Services.JWT;
using RentZ.Application.Services.Lookups;
using RentZ.Application.Services.User.Security;
using RentZ.Application.Services.Validations;

namespace RentZ.Application.Configurations;

public static class ApplicationConfiguration
{
	public static void ServiceConfiguration(this IServiceCollection services)
	{
		services.AddScoped<IUserSecurityService, UserSecurityService>();
		services.AddScoped<IValidations, Validations>();
		services.AddScoped<ILookupService, LookupService>();
		services.AddScoped<IJwtService, JwtService>();
	}
}
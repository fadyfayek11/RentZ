using Microsoft.Extensions.DependencyInjection;
using RentZ.Application.Services.User.Security;

namespace RentZ.Application.Configurations;

public static class ApplicationConfiguration
{
	public static void ServiceConfiguration(this IServiceCollection services)
	{
		services.AddScoped<IUserSecurityService, UserSecurityService>();
	}
}
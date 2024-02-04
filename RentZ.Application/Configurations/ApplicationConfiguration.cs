using ExtCore.FileStorage.Abstractions;
using ExtCore.FileStorage.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using RentZ.Application.Services.Admin;
using RentZ.Application.Services.Feedback;
using RentZ.Application.Services.Files;
using RentZ.Application.Services.JWT;
using RentZ.Application.Services.Lookups;
using RentZ.Application.Services.Messages;
using RentZ.Application.Services.Notification;
using RentZ.Application.Services.Property;
using RentZ.Application.Services.User.Security;
using RentZ.Application.Services.Validations;

namespace RentZ.Application.Configurations;

public static class ApplicationConfiguration
{
	public static void ServiceConfiguration(this IServiceCollection services)
	{
		services.AddScoped<IUserSecurityService, UserSecurityService>();
		services.AddScoped<IPropertyService, PropertyService>();
		services.AddScoped<IValidations, Validations>();
		services.AddScoped<ILookupService, LookupService>();
		services.AddScoped<IJwtService, JwtService>();
        services.AddTransient<IFileManager, FileManager>();
        services.AddTransient<IFileStorage, FileStorage>();
        services.AddTransient<IContentTypeProvider, FileExtensionContentTypeProvider>();
        services.AddTransient<IFeedbackServices, FeedbackServices>();
        services.AddTransient<IAdminServices, AdminServices>();
        services.AddTransient<INotificationService, NotificationService>();
        services.AddTransient<IMessagesService, MessagesService>();
    }
}
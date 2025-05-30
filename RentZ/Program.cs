using System.Text;
using System.Threading.RateLimiting;
using ExtCore.FileStorage;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RentZ.API.Controllers;
using RentZ.Application.Configurations;
using RentZ.Application.Hubs;
using RentZ.Domain.Entities;
using RentZ.DTO.JWT;
using RentZ.Infrastructure.Context;
using IUserIdProvider = Microsoft.AspNetCore.SignalR.IUserIdProvider;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")).UseLazyLoadingProxies().EnableSensitiveDataLogging().EnableDetailedErrors().EnableServiceProviderCaching());

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
	{
		options.SignIn.RequireConfirmedAccount = true;
		options.User.RequireUniqueEmail = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
    })
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RentZ", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddMemoryCache();
builder.Services.AddOptions();


builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddPolicy("property-view", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Extract property ID from query string
        var propertyId = httpContext.Request.Query["PropId"].ToString() ?? "none";

        var partitionKey = $"{ip}:{propertyId}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: partitionKey,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1,
                Window = TimeSpan.FromDays(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    rateLimiterOptions.AddPolicy("limitDay", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.Id,
        factory: _ => new FixedWindowRateLimiterOptions()
        {
            PermitLimit = 1,
            Window = TimeSpan.FromDays(1),

        }));
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RootAdminOnly", policy => policy.RequireRole("RootAdmin"));
    options.AddPolicy("ClientOnly", policy => policy.RequireRole("Client"));
});

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters()
	{
		ValidateActor = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
		ValidAudience = builder.Configuration["JwtSettings:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
	};
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken)
                && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;

                context.HttpContext.Items["ChatId"] = context.Request.Query["chat_id"];
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.ServiceConfiguration();
builder.Services.AddFluentValidationRulesToSwagger();
builder.Services.AddHttpContextAccessor();

// 6. Add CORS policy
builder.Services.AddCors();
builder.Services.Configure<FileStorageOptions>(options =>
{
    options.RootPath = $"{builder.Environment.ContentRootPath}\\Documents\\";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//	app.UseSwagger();
//	app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseCors(policy =>
    policy.WithOrigins("localhost:4200",
            "https://www.berveh.com/", "http://www.berveh.com/", "https://www.berveh.com/RentzAdmin",
            "https://41.196.0.80/", "http://41.196.0.80/").AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("chatHub");

app.Run();

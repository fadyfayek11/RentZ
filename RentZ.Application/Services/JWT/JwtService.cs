using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RentZ.DTO.JWT;

namespace RentZ.Application.Services.JWT;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<Domain.Entities.User> _userManager;


    public JwtService(IOptions<JwtSettings> jwtSettings, UserManager<Domain.Entities.User> userManager)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
    }
    public GenerateTokenResponseDto GenerateToken(GenerateTokenRequestDto tokenRequest)
    {
        var role = _userManager.GetRolesAsync(new Domain.Entities.User()
        {
            Id = Guid.Parse(tokenRequest.UserId)
        }).Result.FirstOrDefault();

        var claims = new[]
        {
            new Claim("UserId", tokenRequest.UserId),
            new Claim(JwtRegisteredClaimNames.Name, tokenRequest.DisplayName ?? ""),
            new Claim(JwtRegisteredClaimNames.Email, tokenRequest.UserEmail ?? ""),
            new Claim("PhoneNumber", tokenRequest.PhoneNumber ?? ""),
            new Claim("Gender", tokenRequest.Gender.ToString() ?? ""),
            new Claim("FavLang", tokenRequest.FavLang.ToString() ?? ""),
            new Claim("IsOwner", tokenRequest.IsOwner.ToString() ?? ""),
            new Claim("IsActiveAcc", tokenRequest.IsActive.ToString() ?? ""),
            new Claim("OtpVerified", tokenRequest.IsOtpVerified.ToString() ?? ""),
            new Claim(ClaimTypes.Role, role ?? ""),
            new Claim("UserImage", tokenRequest.UserImage ?? ""),
        };

	    var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddYears(1),
            signingCredentials: signingCredentials
        );
        return new GenerateTokenResponseDto(new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken), jwtSecurityToken.ValidTo);
    }

}


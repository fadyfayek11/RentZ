using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RentZ.DTO.JWT;

namespace RentZ.Application.Services.JWT;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }
    public GenerateTokenResponseDto GenerateToken(GenerateTokenRequestDto tokenRequest)
    {
        var claims = new[]
        {
            new System.Security.Claims.Claim("UserId", tokenRequest.UserId),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Name, tokenRequest.UserName),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, tokenRequest.UserEmail),
            new System.Security.Claims.Claim("PhoneNumber", tokenRequest.PhoneNumber),
            new System.Security.Claims.Claim("Gender", tokenRequest.Gender.ToString()),
            new System.Security.Claims.Claim("Bio", tokenRequest.Bio ?? ""),
            new System.Security.Claims.Claim("FavLang", tokenRequest.FavLang.ToString()),
            new System.Security.Claims.Claim("City", tokenRequest.City),
            new System.Security.Claims.Claim("IsOwner", tokenRequest.IsOwner.ToString()),
            new System.Security.Claims.Claim("IsActiveAcc", tokenRequest.IsActive.ToString()),
            new System.Security.Claims.Claim("OtpVerified", tokenRequest.IsOtpVerified.ToString()),
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


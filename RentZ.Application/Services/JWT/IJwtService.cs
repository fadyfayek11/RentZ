using RentZ.DTO.JWT;

namespace RentZ.Application.Services.JWT
{
    public interface IJwtService
    {
	    GenerateTokenResponseDto GenerateToken(GenerateTokenRequestDto tokenRequest);
    }
}

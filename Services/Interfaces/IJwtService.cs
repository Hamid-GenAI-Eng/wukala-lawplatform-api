using UserAuthAPI.Data.Entities;

namespace UserAuthAPI.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
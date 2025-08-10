using UserAuthAPI.Data.Entities;

namespace UserAuthAPI.Data.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByGoogleIdAsync(string googleId);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> GoogleIdExistsAsync(string googleId);
    Task<IEnumerable<User>> GetAllAsync();
    Task<bool> DeleteAsync(Guid id);
}
using UserAuthAPI.DTOs;

namespace UserAuthAPI.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> SignupAsync(SignupRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request);
    Task<bool> ValidateGmailAddressAsync(string email);
}
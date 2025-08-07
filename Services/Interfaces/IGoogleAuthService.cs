using Google.Apis.Auth;

namespace UserAuthAPI.Services.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken);
    Task<bool> IsValidGmailAddressAsync(string email);
}
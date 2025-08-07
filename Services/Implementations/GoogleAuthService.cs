using Google.Apis.Auth;
using System.Net.Http;
using System.Text.Json;
using UserAuthAPI.Services.Interfaces;

namespace UserAuthAPI.Services.Implementations;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _googleClientId;

    public GoogleAuthService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _googleClientId = _configuration["Google:ClientId"] ?? throw new ArgumentNullException("Google:ClientId");
    }

    public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            
            // Additional validation
            if (payload.EmailVerified != true)
            {
                return null; // Email not verified
            }

            return payload;
        }
        catch (Exception ex)
        {
            // Log the exception (in a real app, use proper logging)
            Console.WriteLine($"Google token validation error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> IsValidGmailAddressAsync(string email)
    {
        // Check if it's a Gmail address
        if (!IsGmailAddress(email))
        {
            return false;
        }

        try
        {
            // Use Gmail API to check if the email exists
            // For production, you'd need proper Gmail API credentials
            // For now, we'll do basic validation and assume Gmail addresses are valid
            
            // You can implement more sophisticated validation here:
            // 1. Use Gmail API to check if email exists
            // 2. Send verification email
            // 3. Use third-party email validation services
            
            return await ValidateEmailWithGmailAPI(email);
        }
        catch
        {
            // If API call fails, fall back to basic validation
            return IsGmailAddress(email);
        }
    }

    private bool IsGmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var emailLower = email.ToLower().Trim();
        return emailLower.EndsWith("@gmail.com") && 
               emailLower.Length > 10 && // Minimum length for a Gmail address
               IsValidEmailFormat(email);
    }

    private bool IsValidEmailFormat(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> ValidateEmailWithGmailAPI(string email)
    {
        try
        {
            // This is a simplified implementation
            // In a production environment, you would:
            // 1. Use Google's People API or Gmail API
            // 2. Have proper OAuth 2.0 setup
            // 3. Handle rate limiting and errors properly
            
            // For now, we'll use a basic HTTP check to see if the email format is valid
            // and assume Gmail addresses are generally valid
            
            var emailParts = email.Split('@');
            if (emailParts.Length != 2 || emailParts[0].Length == 0)
                return false;

            var username = emailParts[0];
            
            // Basic Gmail username validation rules
            if (username.Length < 6 || username.Length > 30)
                return false;

            // Gmail usernames can only contain letters, numbers, and dots
            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9.]+$"))
                return false;

            // Can't start or end with a dot
            if (username.StartsWith(".") || username.EndsWith("."))
                return false;

            // Can't have consecutive dots
            if (username.Contains(".."))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
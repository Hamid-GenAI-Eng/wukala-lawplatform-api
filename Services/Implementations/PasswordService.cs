using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using UserAuthAPI.Data.Entities;
using UserAuthAPI.Services.Interfaces;

namespace UserAuthAPI.Services.Implementations;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher;

    public PasswordService()
    {
        _passwordHasher = new PasswordHasher<User>();
    }

    public string HashPassword(string password)
    {
        var user = new User(); // Dummy user for hashing
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var user = new User(); // Dummy user for verification
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
        return result == PasswordVerificationResult.Success || 
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }

    public bool IsStrongPassword(string password)
    {
        return GetPasswordErrors(password).Count == 0;
    }

    public List<string> GetPasswordErrors(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
            return errors;
        }

        if (password.Length < 8)
        {
            errors.Add("Password must be at least 8 characters long.");
        }

        if (password.Length > 100)
        {
            errors.Add("Password must not exceed 100 characters.");
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }

        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            errors.Add("Password must contain at least one lowercase letter.");
        }

        if (!Regex.IsMatch(password, @"\d"))
        {
            errors.Add("Password must contain at least one digit.");
        }

        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
        {
            errors.Add("Password must contain at least one special character.");
        }

        // Check for common weak patterns
        if (ContainsCommonPatterns(password))
        {
            errors.Add("Password contains common patterns and is too weak.");
        }

        return errors;
    }

    private bool ContainsCommonPatterns(string password)
    {
        var commonPatterns = new[]
        {
            "password", "123456", "qwerty", "abc123", "letmein",
            "welcome", "monkey", "dragon", "master", "admin"
        };

        var lowerPassword = password.ToLower();
        
        foreach (var pattern in commonPatterns)
        {
            if (lowerPassword.Contains(pattern))
            {
                return true;
            }
        }

        // Check for sequential characters
        if (Regex.IsMatch(password, @"(012|123|234|345|456|567|678|789|890|abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz)"))
        {
            return true;
        }

        // Check for repeated characters
        if (Regex.IsMatch(password, @"(.)\1{2,}"))
        {
            return true;
        }

        return false;
    }
}
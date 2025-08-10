using FluentValidation;
using UserAuthAPI.DTOs;

namespace UserAuthAPI.Validators;

public class SignupRequestValidator : AbstractValidator<SignupRequest>
{
    public SignupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters.")
            .Matches(@"^[a-zA-Z\s]+$").WithMessage("Name can only contain letters and spaces.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.")
            .Must(BeGmailAddress).WithMessage("Only Gmail addresses (@gmail.com) are allowed.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.")
            .Must(HaveUppercase).WithMessage("Password must contain at least one uppercase letter.")
            .Must(HaveLowercase).WithMessage("Password must contain at least one lowercase letter.")
            .Must(HaveDigit).WithMessage("Password must contain at least one digit.")
            .Must(HaveSpecialCharacter).WithMessage("Password must contain at least one special character.");
    }

    private bool BeGmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return email.ToLower().EndsWith("@gmail.com");
    }

    private bool HaveUppercase(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);
    }

    private bool HaveLowercase(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsLower);
    }

    private bool HaveDigit(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);
    }

    private bool HaveSpecialCharacter(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        var specialCharacters = "!@#$%^&*()_+-=[]{}|;':\",./<>?";
        return password.Any(c => specialCharacters.Contains(c));
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.");
    }
}

public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google ID token is required.")
            .MinimumLength(50).WithMessage("Invalid Google ID token format.");
    }
}
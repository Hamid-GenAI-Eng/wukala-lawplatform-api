using UserAuthAPI.Data.Entities;
using UserAuthAPI.Data.Interfaces;
using UserAuthAPI.DTOs;
using UserAuthAPI.Services.Interfaces;

namespace UserAuthAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IGoogleAuthService _googleAuthService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        IGoogleAuthService googleAuthService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _googleAuthService = googleAuthService;
    }

    public async Task<AuthResponse> SignupAsync(SignupRequest request)
    {
        try
        {
            // Validate Gmail address
            if (!await ValidateGmailAddressAsync(request.Email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid Gmail address. Only valid Gmail addresses (@gmail.com) are allowed.",
                    Errors = new List<string> { "Email must be a valid Gmail address" }
                };
            }

            // Check if user already exists
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "User with this email already exists.",
                    Errors = new List<string> { "Email already registered" }
                };
            }

            // Validate password strength
            var passwordErrors = _passwordService.GetPasswordErrors(request.Password);
            if (passwordErrors.Any())
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Password does not meet security requirements.",
                    Errors = passwordErrors
                };
            }

            // Create new user
            var hashedPassword = _passwordService.HashPassword(request.Password);
            var user = new User
            {
                Name = request.Name.Trim(),
                Email = request.Email.ToLower().Trim(),
                PasswordHash = hashedPassword,
                Provider = "Local"
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // Generate JWT token
            var token = _jwtService.GenerateToken(createdUser);

            return new AuthResponse
            {
                Success = true,
                Message = "User registered successfully.",
                Token = token,
                User = new UserDto
                {
                    Id = createdUser.Id,
                    Name = createdUser.Name,
                    Email = createdUser.Email,
                    Provider = createdUser.Provider,
                    CreatedAt = createdUser.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred during registration.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password.",
                    Errors = new List<string> { "Authentication failed" }
                };
            }

            // Check if user is a local user (not Google user)
            if (user.Provider != "Local" || string.IsNullOrEmpty(user.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "This account was created using Google Sign-In. Please use Google login.",
                    Errors = new List<string> { "Invalid login method" }
                };
            }

            // Verify password
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password.",
                    Errors = new List<string> { "Authentication failed" }
                };
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Provider = user.Provider,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred during login.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        try
        {
            // Verify Google token
            var payload = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
            if (payload == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid Google token.",
                    Errors = new List<string> { "Google authentication failed" }
                };
            }

            // Check if it's a Gmail address
            if (!await ValidateGmailAddressAsync(payload.Email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Only Gmail addresses are allowed.",
                    Errors = new List<string> { "Email must be a Gmail address" }
                };
            }

            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(payload.Email);
            
            User user;
            string message;

            if (existingUser != null)
            {
                // User exists - update Google ID if needed
                if (existingUser.Provider == "Local")
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "An account with this email already exists. Please use email/password login.",
                        Errors = new List<string> { "Account exists with different provider" }
                    };
                }

                user = existingUser;
                if (user.GoogleId != payload.Subject)
                {
                    user.GoogleId = payload.Subject;
                    user = await _userRepository.UpdateAsync(user);
                }
                message = "Login successful.";
            }
            else
            {
                // Create new user from Google account
                user = new User
                {
                    Name = payload.Name ?? payload.GivenName ?? "User",
                    Email = payload.Email.ToLower().Trim(),
                    Provider = "Google",
                    GoogleId = payload.Subject
                };

                user = await _userRepository.CreateAsync(user);
                message = "Account created and login successful.";
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            return new AuthResponse
            {
                Success = true,
                Message = message,
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Provider = user.Provider,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred during Google authentication.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<bool> ValidateGmailAddressAsync(string email)
    {
        return await _googleAuthService.IsValidGmailAddressAsync(email);
    }
}
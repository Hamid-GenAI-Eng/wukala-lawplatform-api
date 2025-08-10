using Microsoft.AspNetCore.Mvc;
using UserAuthAPI.DTOs;
using UserAuthAPI.Services.Interfaces;

namespace UserAuthAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user with email and password
    /// </summary>
    /// <param name="request">User registration details</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("signup")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Signup([FromBody] SignupRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(errors));
            }

            var result = await _authService.SignupAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("User {Email} registered successfully", request.Email);
                return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "User registered successfully"));
            }

            _logger.LogWarning("Registration failed for {Email}: {Message}", request.Email, result.Message);
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(result.Message, result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Email}", request.Email);
            return StatusCode(500, ApiResponse<AuthResponse>.ErrorResponse("An internal error occurred during registration"));
        }
    }

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(errors));
            }

            var result = await _authService.LoginAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("User {Email} logged in successfully", request.Email);
                return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful"));
            }

            _logger.LogWarning("Login failed for {Email}: {Message}", request.Email, result.Message);
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse(result.Message, result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login for {Email}", request.Email);
            return StatusCode(500, ApiResponse<AuthResponse>.ErrorResponse("An internal error occurred during login"));
        }
    }

    /// <summary>
    /// Authenticate user with Google ID token
    /// </summary>
    /// <param name="request">Google authentication details</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("google-login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(errors));
            }

            var result = await _authService.GoogleLoginAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("Google login successful for user {Email}", result.User?.Email);
                return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, result.Message));
            }

            _logger.LogWarning("Google login failed: {Message}", result.Message);
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(result.Message, result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return StatusCode(500, ApiResponse<AuthResponse>.ErrorResponse("An internal error occurred during Google authentication"));
        }
    }

    /// <summary>
    /// Validate if an email is a valid Gmail address
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate-gmail")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateGmail([FromQuery] string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Email is required"));
            }

            var isValid = await _authService.ValidateGmailAddressAsync(email);
            var message = isValid ? "Valid Gmail address" : "Invalid Gmail address";
            
            return Ok(ApiResponse<bool>.SuccessResponse(isValid, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Gmail address {Email}", email);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred during email validation"));
        }
    }
}
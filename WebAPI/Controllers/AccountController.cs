using AutoMapper;
using BusinessObject.DTOs;
using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using System.Security.Claims;
using WebAPI.Services;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserRepository userRepo, ITokenService tokenService, IMapper mapper, IConfiguration config, ILogger<AccountController> logger)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _mapper = mapper;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _userRepo.GetByEmailAsync(request.Email) != null)
                return BadRequest("Email already exists");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Role = "Employee"
            };

            await _userRepo.AddAsync(user);

            return Ok("Registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);
            if (user == null || user.PasswordHash != HashPassword(request.Password))
                return Unauthorized("Invalid credentials");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            int refreshDays = int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7");
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(refreshDays)
            });

            await _userRepo.UpdateAsync(user);

            return Ok(new AuthResponse
  {
      AccessToken = accessToken,
      RefreshToken = refreshToken
  });
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(AuthResponse request)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            var email = principal?.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized("Invalid access token");

            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null) return Unauthorized();

            var storedToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
            if (storedToken == null || storedToken.ExpiryDate < DateTime.UtcNow)
                return Unauthorized("Invalid refresh token");

            user.RefreshTokens.Remove(storedToken);

            var newRefresh = _tokenService.GenerateRefreshToken();
            int refreshDays = int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7");

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefresh,
                ExpiryDate = DateTime.UtcNow.AddDays(refreshDays)
            });

            await _userRepo.UpdateAsync(user);

            var newAccess = _tokenService.GenerateAccessToken(user);
            return Ok(new AuthResponse
            {
                AccessToken = newAccess,
                RefreshToken = newRefresh
            });
        }


        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }


        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                _logger.LogInformation($"Getting current user for email: {email}");

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email not found in token");
                    return Unauthorized(ApiResponseDto.ErrorResult("Invalid token"));
                }

                var user = await _userRepo.GetByEmailAsync(email);
                if (user == null) 
                {
                    _logger.LogWarning($"User not found for email: {email}");
                    return NotFound(ApiResponseDto.ErrorResult("User not found"));
                }

                var dto = _mapper.Map<UserDTO>(user);
                _logger.LogInformation($"Successfully retrieved user: {user.FullName}");
                return Ok(ApiResponseDto.SuccessResult(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user information");
                return StatusCode(500, ApiResponseDto.ErrorResult("Error getting user information"));
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null) return NotFound();
            // Xóa hết refresh token của user
            user.RefreshTokens.Clear();
            await _userRepo.UpdateAsync(user);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-area")]
        public IActionResult OnlyAdmin()
        {
            return Ok("Welcome, Admin!");
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                _logger.LogInformation($"ChangePassword called with request: CurrentPassword={!string.IsNullOrEmpty(request?.CurrentPassword)}, NewPassword={!string.IsNullOrEmpty(request?.NewPassword)}");
                
                // Validate request
                if (request == null)
                {
                    _logger.LogWarning("ChangePassword request is null");
                    return BadRequest(ApiResponseDto.ErrorResult("Request is null"));
                }

                if (string.IsNullOrEmpty(request.CurrentPassword))
                {
                    _logger.LogWarning("CurrentPassword is null or empty");
                    return BadRequest(ApiResponseDto.ErrorResult("Current password is required"));
                }

                if (string.IsNullOrEmpty(request.NewPassword))
                {
                    _logger.LogWarning("NewPassword is null or empty");
                    return BadRequest(ApiResponseDto.ErrorResult("New password is required"));
                }

                if (request.NewPassword.Length < 6)
                {
                    _logger.LogWarning("NewPassword is too short");
                    return BadRequest(ApiResponseDto.ErrorResult("New password must be at least 6 characters"));
                }

                // Get current user ID from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("Invalid user token for password change");
                    return Unauthorized(ApiResponseDto.ErrorResult("Invalid user token"));
                }

                _logger.LogInformation($"User ID from token: {userId}");

                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User not found for ID: {userId}");
                    return NotFound(ApiResponseDto.ErrorResult("User not found"));
                }

                _logger.LogInformation($"Found user: {user.Email}");

                // Verify current password
                var currentPasswordHash = HashPassword(request.CurrentPassword);
                _logger.LogInformation($"Current password hash: {currentPasswordHash}, User password hash: {user.PasswordHash}");
                
                if (user.PasswordHash != currentPasswordHash)
                {
                    _logger.LogWarning($"Incorrect current password for user: {user.Email}");
                    return BadRequest(ApiResponseDto.ErrorResult("Current password is incorrect"));
                }

                // Update password
                user.PasswordHash = HashPassword(request.NewPassword);
                await _userRepo.UpdateAsync(user);

                _logger.LogInformation($"Password changed successfully for user: {user.Email}");
                return Ok(ApiResponseDto.SuccessResult("Password changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, ApiResponseDto.ErrorResult("Error changing password"));
            }
        }

        [Authorize]
        [HttpPost("test-change-password")]
        public async Task<IActionResult> TestChangePassword([FromBody] object request)
        {
            try
            {
                _logger.LogInformation($"TestChangePassword called with request: {System.Text.Json.JsonSerializer.Serialize(request)}");
                return Ok(new { message = "Test endpoint working", request = request });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test endpoint");
                return StatusCode(500, ApiResponseDto.ErrorResult("Error in test endpoint"));
            }
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            try
            {
                // Get current user ID from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("Invalid user token for profile update");
                    return Unauthorized(ApiResponseDto.ErrorResult("Invalid user token"));
                }

                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User not found for ID: {userId}");
                    return NotFound(ApiResponseDto.ErrorResult("User not found"));
                }

                // Check if email is already taken by another user
                var existingUserWithEmail = await _userRepo.GetByEmailAsync(request.Email);
                if (existingUserWithEmail != null && existingUserWithEmail.Id != userId)
                {
                    _logger.LogWarning($"Email {request.Email} is already taken by another user");
                    return BadRequest(ApiResponseDto.ErrorResult("Email is already taken by another user"));
                }

                // Update user profile
                user.FullName = request.FullName;
                user.Email = request.Email;
                await _userRepo.UpdateAsync(user);

                _logger.LogInformation($"Profile updated successfully for user: {user.Email}");
                return Ok(ApiResponseDto.SuccessResult("Profile updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, ApiResponseDto.ErrorResult("Error updating profile"));
            }
        }

    }
}

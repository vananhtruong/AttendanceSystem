using AutoMapper;
using BusinessObject.DTOs;
using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using System.Security.Claims;
using WebAPI.Services;

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

        public AccountController(IUserRepository userRepo, ITokenService tokenService, IMapper mapper, IConfiguration config)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _mapper = mapper;
            _config = config;
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
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid token");

            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null) return NotFound();

            var dto = _mapper.Map<UserDTO>(user);
            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-area")]
        public IActionResult OnlyAdmin()
        {
            return Ok("Welcome, Admin!");
        }





    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using BusinessObject.Models;
using BusinessObject.DTOs;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceRecordRepository _attendanceRepository;
        private readonly IUserRepository _userRepository;

        public AttendanceController(
            IAttendanceRecordRepository attendanceRepository,
            IUserRepository userRepository)
        {
            _attendanceRepository = attendanceRepository;
            _userRepository = userRepository;
        }

        [HttpGet("my-attendance")]
        public async Task<IActionResult> GetMyAttendance([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return NotFound();

            var (start, end) = GetDateRange(startDate, endDate);
            var records = await _attendanceRepository.GetByDateRangeAsync(start, end);
            var userRecords = records.Where(r => r.UserId == user.Id).ToList();

            return Ok(ApiResponseDto.SuccessResult(userRecords));
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAttendance([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var (start, end) = GetDateRange(startDate, endDate);
            var records = await _attendanceRepository.GetByDateRangeAsync(start, end);
            return Ok(ApiResponseDto.SuccessResult(records));
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserAttendance(int userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var (start, end) = GetDateRange(startDate, endDate);
            var records = await _attendanceRepository.GetByDateRangeAsync(start, end);
            var userRecords = records.Where(r => r.UserId == userId).ToList();

            return Ok(ApiResponseDto.SuccessResult(userRecords));
        }

        private (DateTime start, DateTime end) GetDateRange(DateTime? startDate, DateTime? endDate)
        {
            return (startDate ?? DateTime.Today.AddDays(-30), endDate ?? DateTime.Today);
        }
    }
} 
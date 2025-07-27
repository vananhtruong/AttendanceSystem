using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using BusinessObject.Models;
using AutoMapper;
using BusinessObject.DTOs;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IWorkScheduleRepository _workScheduleRepo;
        private readonly IAttendanceRecordRepository _attendanceRepo;
        private readonly IMapper _mapper;

        public AttendanceController(IUserRepository userRepo, IWorkScheduleRepository workScheduleRepo, IAttendanceRecordRepository attendanceRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _workScheduleRepo = workScheduleRepo;
            _attendanceRepo = attendanceRepo;
            _mapper = mapper;
        }

        // GET: api/Attendance/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var records = await _attendanceRepo.GetByUserIdAsync(userId);
            var dto = _mapper.Map<List<AttendanceRecordDTO>>(records);
            return Ok(dto);
        }

        // GET: api/Attendance/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _attendanceRepo.GetByIdAsync(id);
            if (record == null) return NotFound();
            var dto = _mapper.Map<AttendanceRecordDTO>(record);
            return Ok(dto);
        }

        // GET: api/Attendance/count
        [HttpGet("count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAttendanceCount()
        {
            var records = await _attendanceRepo.GetAllAsync();
            var count = records.Count();
            return Ok(count);
        }

        // POST: api/Attendance/checkin
        [HttpPost("checkin")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> CheckIn([FromBody] AttendanceRecordDTO dto)
        {
            // Giả lập nhận diện khuôn mặt: chỉ nhận thông tin checkin, userId, workScheduleId, date
            var record = _mapper.Map<AttendanceRecord>(dto);
            record.Status = "CheckedIn";
            record.CheckInTime = DateTime.UtcNow;
            await _attendanceRepo.AddAsync(record);
            return Ok();
        }

        // PUT: api/Attendance/checkout/{id}
        [HttpPut("checkout/{id}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> CheckOut(int id)
        {
            var record = await _attendanceRepo.GetByIdAsync(id);
            if (record == null) return NotFound();
            record.CheckOutTime = DateTime.UtcNow;
            record.Status = "CheckedOut";
            // Tính số giờ làm
            if (record.CheckOutTime.HasValue)
                record.HoursWorked = (decimal)(record.CheckOutTime.Value - record.CheckInTime).TotalHours;
            await _attendanceRepo.UpdateAsync(record);
            return Ok();
        }

        // PUT: api/Attendance/status/{id}
        [HttpPut("status/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var record = await _attendanceRepo.GetByIdAsync(id);
            if (record == null) return NotFound();
            record.Status = status;
            await _attendanceRepo.UpdateAsync(record);
            return Ok();

        }
    }
} 
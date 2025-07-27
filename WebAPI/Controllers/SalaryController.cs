using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using BusinessObject.Models;
using AutoMapper;
using BusinessObject.DTOs;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalaryController : ControllerBase
    {
        private readonly ISalaryRepository _salaryRepo;
        private readonly IUserRepository _userRepo;
        private readonly IAttendanceRecordRepository _attendanceRepo;
        private readonly IMapper _mapper;

        public SalaryController(ISalaryRepository salaryRepo, IUserRepository userRepo, IAttendanceRecordRepository attendanceRepo, IMapper mapper)
        {
            _salaryRepo = salaryRepo;
            _userRepo = userRepo;
            _attendanceRepo = attendanceRepo;
            _mapper = mapper;
        }

        // GET: api/Salary/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var salaries = await _salaryRepo.GetAllAsync();
            var result = salaries.Where(s => s.UserId == userId).ToList();
            var dto = _mapper.Map<List<SalaryRecordDTO>>(result);
            return Ok(dto);
        }

        // GET: api/Salary/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetById(int id)
        {
            var salary = await _salaryRepo.GetByIdAsync(id);
            if (salary == null) return NotFound();
            var dto = _mapper.Map<SalaryRecordDTO>(salary);
            return Ok(dto);
        }

        // POST: api/Salary/calculate
        [HttpPost("calculate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CalculateSalary([FromBody] int userId)
        {
            var salary = await _salaryRepo.CalculateSalaryAsync(userId, DateTime.UtcNow);
            var dto = _mapper.Map<SalaryRecordDTO>(salary);
            return Ok(dto);
        }

        // GET: api/Salary/count
        [HttpGet("count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSalaryCount()
        {
            var salaries = await _salaryRepo.GetAllAsync();
            var count = salaries.Count();
            return Ok(count);
        }
    }
} 
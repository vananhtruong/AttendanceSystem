using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using AutoMapper;
using BusinessObject.DTOs;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class EmployeeController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public EmployeeController(IUserRepository userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }

        // GET: api/Employee
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _userRepo.GetAllAsync();
            var result = employees.Where(e => e.Role == "Employee").ToList();
            var dto = _mapper.Map<List<UserDTO>>(result);
            return Ok(dto);
        }

        // GET: api/Employee/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null || user.Role != "Employee")
                return NotFound();
            var dto = _mapper.Map<UserDTO>(user);
            return Ok(dto);
        }

        // PUT: api/Employee/ban/{id}
        [HttpPut("ban/{id}")]
        public async Task<IActionResult> BanEmployee(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null || user.Role != "Employee")
                return NotFound();
            if (user.IsBanned)
                return BadRequest("Employee is already banned.");
            user.IsBanned = true;
            await _userRepo.UpdateUserAsync(user);
            return Ok("Employee banned successfully");
        }

        // PUT: api/Employee/unban/{id}
        [HttpPut("unban/{id}")]
        public async Task<IActionResult> UnbanEmployee(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null || user.Role != "Employee")
                return NotFound();
            if (!user.IsBanned)
                return BadRequest("Employee is not banned.");
            user.IsBanned = false;
            await _userRepo.UpdateUserAsync(user);
            return Ok("Employee unbanned successfully");
        }

        // GET: api/Employee/search?keyword=abc
        [HttpGet("search")]
        public async Task<IActionResult> SearchEmployee([FromQuery] string keyword)
        {
            var employees = await _userRepo.SearchAsync(keyword);
            var result = employees.Where(e => e.Role == "Employee").ToList();
            var dto = _mapper.Map<List<UserDTO>>(result);
            return Ok(dto);
        }
        [HttpGet("count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEmployeeCount()
        {
            var employees = await _userRepo.GetAllAsync();
            var count = employees.Count(e => e.Role == "Employee");
            return Ok(count);
        }
    }
} 
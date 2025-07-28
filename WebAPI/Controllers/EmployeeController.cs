using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using AutoMapper;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.OData.Query;

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
        [EnableQuery]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var employees = await _userRepo.GetAllAsync();
                var result = employees.Where(e => e.Role == "Employee").ToList();
                var dto = _mapper.Map<List<UserDTO>>(result);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // GET: api/Employee/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(id);
                if (user == null || user.Role != "Employee")
                    return NotFound(new { error = "Employee not found" });
                var dto = _mapper.Map<UserDTO>(user);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // PUT: api/Employee/ban/{id}
        [HttpPut("ban/{id}")]
        public async Task<IActionResult> BanEmployee(int id)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(id);
                if (user == null || user.Role != "Employee")
                    return NotFound(new { error = "Employee not found" });
                if (user.IsBanned)
                    return BadRequest(new { error = "Employee is already banned" });
                user.IsBanned = true;
                await _userRepo.UpdateUserAsync(user);
                return Ok(new { message = "Employee banned successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // PUT: api/Employee/unban/{id}
        [HttpPut("unban/{id}")]
        public async Task<IActionResult> UnbanEmployee(int id)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(id);
                if (user == null || user.Role != "Employee")
                    return NotFound(new { error = "Employee not found" });
                if (!user.IsBanned)
                    return BadRequest(new { error = "Employee is not banned" });
                user.IsBanned = false;
                await _userRepo.UpdateUserAsync(user);
                return Ok(new { message = "Employee unbanned successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // GET: api/Employee/search?keyword=abc
        [HttpGet("search")]
        public async Task<IActionResult> SearchEmployee([FromQuery] string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return await GetAllEmployees();
                }

                var employees = await _userRepo.SearchAsync(keyword);
                var result = employees.Where(e => e.Role == "Employee").ToList();
                var dto = _mapper.Map<List<UserDTO>>(result);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEmployeeCount()
        {
            try
            {
                var employees = await _userRepo.GetAllAsync();
                var count = employees.Count(e => e.Role == "Employee");
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
} 
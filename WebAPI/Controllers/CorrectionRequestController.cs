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
    public class CorrectionRequestController : ControllerBase
    {
        private readonly ICorrectionRequestRepository _correctionRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public CorrectionRequestController(ICorrectionRequestRepository correctionRepo, IUserRepository userRepo, IMapper mapper)
        {
            _correctionRepo = correctionRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        // GET: api/CorrectionRequest/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var requests = await _correctionRepo.GetByUserIdAsync(userId);
            var dto = _mapper.Map<List<CorrectionRequestDTO>>(requests);
            return Ok(dto);
        }

        // GET: api/CorrectionRequest
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _correctionRepo.GetAllAsync();
            var dto = _mapper.Map<List<CorrectionRequestDTO>>(requests);
            return Ok(dto);
        }

        // POST: api/CorrectionRequest
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Create([FromBody] CorrectionRequestDTO dto)
        {
            var request = _mapper.Map<CorrectionRequest>(dto);
            request.Status = "Pending";
            request.Date = DateTime.UtcNow;
            await _correctionRepo.AddAsync(request);
            return Ok();
        }

        // PUT: api/CorrectionRequest/approve/{id}
        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _correctionRepo.GetByIdAsync(id);
            if (request == null) return NotFound();
            request.Status = "Approved";
            await _correctionRepo.UpdateAsync(request);
            return Ok();
        }

        // PUT: api/CorrectionRequest/reject/{id}
        [HttpPut("reject/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _correctionRepo.GetByIdAsync(id);
            if (request == null) return NotFound();
            request.Status = "Rejected";
            await _correctionRepo.UpdateAsync(request);
            return Ok();
        }
        [HttpGet("pending/count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingCorrectionRequestCount()
        {
            var requests = await _correctionRepo.GetAllAsync();
            var count = requests.Count(r => r.Status != null && r.Status == "Pending");
            return Ok(count);
        }
    }
} 
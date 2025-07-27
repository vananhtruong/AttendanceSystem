using AutoMapper;
using BusinessObject.DTOs;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkShiftsController : ControllerBase
    {
        private readonly IWorkShiftRepository _repo;
        private readonly IMapper _mapper;

        public WorkShiftsController(IWorkShiftRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkShiftDTO>>> GetAll()
        {
            var shifts = await _repo.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<WorkShiftDTO>>(shifts));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkShiftDTO>> GetById(int id)
        {
            var shift = await _repo.GetByIdAsync(id);
            if (shift == null) return NotFound();
            return Ok(_mapper.Map<WorkShiftDTO>(shift));
        }

        [HttpPost]
        public async Task<ActionResult<WorkShiftDTO>> Create([FromBody] WorkShiftCreateDTO dto)
        {
            var shift = _mapper.Map<WorkShift>(dto);
            var created = await _repo.CreateAsync(shift);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, _mapper.Map<WorkShiftDTO>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WorkShiftUpdateDTO dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            _mapper.Map(dto, existing);
            await _repo.UpdateAsync(existing);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}

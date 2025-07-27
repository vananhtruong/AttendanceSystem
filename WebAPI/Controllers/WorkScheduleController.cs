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
    public class WorkScheduleController : ControllerBase
    {
        private readonly IWorkScheduleRepository _workScheduleRepo;
        private readonly IMapper _mapper;
        private readonly IAttendanceRecordRepository _attendanceRecordRepository;

        public WorkScheduleController(IWorkScheduleRepository workScheduleRepo, IMapper mapper, IAttendanceRecordRepository attendanceRecordRepository)
        {
            _workScheduleRepo = workScheduleRepo;
            _mapper = mapper;
            _attendanceRecordRepository = attendanceRecordRepository;
        }

        // GET: api/WorkSchedule
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var schedules = await _workScheduleRepo.GetAllAsync();
            var dto = _mapper.Map<List<WorkScheduleDTO>>(schedules);
            return Ok(dto);
        }

        // GET: api/WorkSchedule/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var schedule = await _workScheduleRepo.GetByIdAsync(id);
            if (schedule == null) return NotFound();
            var dto = _mapper.Map<WorkScheduleDTO>(schedule);
            return Ok(dto);
        }

        // POST: api/WorkSchedule
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] WorkScheduleDTO dto)
        {
            var schedule = _mapper.Map<WorkSchedule>(dto);
            await _workScheduleRepo.AddAsync(schedule);
            return Ok();
        }

        // PUT: api/WorkSchedule/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] WorkScheduleDTO dto)
        {
            var schedule = await _workScheduleRepo.GetByIdAsync(id);
            if (schedule == null) return NotFound();
            _mapper.Map(dto, schedule);
            await _workScheduleRepo.UpdateAsync(schedule);
            return Ok();
        }

        // DELETE: api/WorkSchedule/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _workScheduleRepo.DeleteAsync(id);
            return Ok();
        }

        // GET: api/WorkSchedule/overtime
        [HttpGet("overtime")]
        public async Task<IActionResult> GetOvertimeSchedules()
        {
            var overtime = await _workScheduleRepo.GetAllAsync();
            var result = overtime.Where(s => s.WorkShift.IsOvertime).ToList();
            var dto = _mapper.Map<List<WorkScheduleDTO>>(result);
            return Ok(dto);
        }

        // GET: api/WorkSchedule/user/{userId}/with-attendance
        [HttpGet("user/{userId}/with-attendance")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetSchedulesWithAttendance(int userId)
        {
            var schedules = await _workScheduleRepo.GetByUserIdAsync(userId);
            var attendanceRecords = await _attendanceRecordRepository.GetByUserIdAsync(userId);
            var result = schedules.Select(s => {
                var attendance = attendanceRecords.FirstOrDefault(a => a.WorkScheduleId == s.Id);
                var shift = s.WorkShift;
                string status;
                if (attendance == null)
                    status = "Absent";
                else if (attendance.CheckInTime > s.WorkDate.Date.Add(shift.StartTime))
                    status = "Late";
                else if (attendance.CheckInTime <= s.WorkDate.Date.Add(shift.StartTime) && attendance.CheckOutTime >= s.WorkDate.Date.Add(shift.EndTime))
                    status = "OnTime";
                else
                    status = "not yet";
                return new WorkScheduleWithAttendanceDTO
                {
                    WorkScheduleId = s.Id,
                    WorkDate = s.WorkDate,
                    ShiftName = shift.Name,
                    IsOvertime = shift.IsOvertime,
                    ScheduleStatus = s.Status,
                    AttendanceStatus = status,
                    CheckInTime = attendance?.CheckInTime,
                    CheckOutTime = attendance?.CheckOutTime
                };
            }).OrderBy(x => x.WorkDate).ToList();
            return Ok(result);
        }
        [HttpGet("count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWorkScheduleCount()
        {
            var schedules = await _workScheduleRepo.GetAllAsync();
            var count = schedules.Count();
            return Ok(count);
        }
    }
} 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using BusinessObject.Models;
using AutoMapper;
using BusinessObject.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkScheduleController : ControllerBase
    {
        private readonly IWorkScheduleRepository _workScheduleRepo;
        private readonly IMapper _mapper;
        private readonly IAttendanceRecordRepository _attendanceRecordRepository;
        private readonly IWorkScheduleEvaluatorService _evaluator;

        public WorkScheduleController(
            IWorkScheduleRepository workScheduleRepo,
            IMapper mapper,
            IAttendanceRecordRepository attendanceRecordRepository,
            IWorkScheduleEvaluatorService evaluator)
        {
            _workScheduleRepo = workScheduleRepo;
            _mapper = mapper;
            _attendanceRecordRepository = attendanceRecordRepository;
            _evaluator = evaluator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var schedules = await _workScheduleRepo.GetAllAsync();
            var dto = _mapper.Map<List<WorkScheduleDTO>>(schedules);
            return Ok(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var schedule = await _workScheduleRepo.GetByIdAsync(id);
            if (schedule == null) return NotFound();
            var dto = _mapper.Map<WorkScheduleDTO>(schedule);
            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] WorkScheduleDTO dto)
        {
            var schedule = _mapper.Map<WorkSchedule>(dto);
            await _workScheduleRepo.AddAsync(schedule);
            return Ok();
        }

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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _workScheduleRepo.DeleteAsync(id);
            return Ok();
        }

        [HttpGet("overtime")]
        public async Task<IActionResult> GetOvertimeSchedules()
        {
            var overtime = await _workScheduleRepo.GetAllAsync();
            var result = overtime.Where(s => s.WorkShift.IsOvertime).ToList();
            var dto = _mapper.Map<List<WorkScheduleDTO>>(result);
            return Ok(dto);
        }

        [HttpGet("user/{userId}/with-attendance")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetSchedulesWithAttendance(int userId)
        {
            var schedules = await _workScheduleRepo.GetByUserIdAsync(userId);
            var attendanceRecords = await _attendanceRecordRepository.GetByUserIdAsync(userId);

            var result = schedules.Select(s =>
            {
                var shift = s.WorkShift;
                var checkIn = attendanceRecords
                    .Where(a => a.WorkScheduleId == s.Id && a.Type == "CheckIn")
                    .OrderBy(a => a.RecordTime)
                    .FirstOrDefault();
                var checkOut = attendanceRecords
                    .Where(a => a.WorkScheduleId == s.Id && a.Type == "CheckOut")
                    .OrderByDescending(a => a.RecordTime)
                    .FirstOrDefault();

                // Tính trạng thái & giờ làm
                (decimal hoursWorked, string scheduleStatus) = _evaluator.EvaluateStatus(
                    s.WorkDate, shift.StartTime, shift.EndTime, checkIn?.RecordTime, checkOut?.RecordTime
                );
                s.Status = scheduleStatus;

                return new WorkScheduleWithAttendanceDTO
                {
                    WorkScheduleId = s.Id,
                    WorkDate = s.WorkDate,
                    ShiftName = shift.Name,
                    IsOvertime = shift.IsOvertime,
                    ScheduleStatus = scheduleStatus,

                    CheckInTime = checkIn?.RecordTime,
                    CheckInStatus = checkIn == null ? "Absent" :
                        checkIn.RecordTime <= s.WorkDate.Date.Add(shift.StartTime) ? "OnTime" : "Late",

                    CheckOutTime = checkOut?.RecordTime,
                    CheckOutStatus = checkOut == null ? "Absent" :
                        checkOut.RecordTime >= s.WorkDate.Date.Add(shift.EndTime) ? "OnTime" : "Early",

                    HoursWorked = Math.Round(hoursWorked, 2),

                    WorkShiftId = s.WorkShiftId,
                    WorkShift = shift
                };
            }).OrderBy(x => x.WorkDate).ToList();

            return Ok(result);
        }


        [HttpGet("count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWorkScheduleCount()
        {
            var schedules = await _workScheduleRepo.GetAllAsync();
            return Ok(schedules.Count());
        }
    }
}

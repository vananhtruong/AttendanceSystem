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
        private readonly IUserRepository _userRepo;
        private readonly IWorkScheduleEvaluatorService _evaluator;

        public WorkScheduleController(
            IWorkScheduleRepository workScheduleRepo,
            IMapper mapper,
            IAttendanceRecordRepository attendanceRecordRepository,
            IUserRepository userRepo,
            IWorkScheduleEvaluatorService evaluator)
        {
            _workScheduleRepo = workScheduleRepo;
            _mapper = mapper;
            _attendanceRecordRepository = attendanceRecordRepository;
            _userRepo = userRepo;
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
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] WorkScheduleCreateDTO dto)
        {
            var schedule = _mapper.Map<WorkSchedule>(dto);
            await _workScheduleRepo.AddAsync(schedule);
            var result = _mapper.Map<WorkScheduleDTO>(schedule);
            return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, result);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] WorkScheduleUpdateDTO dto)
        {
            var schedule = await _workScheduleRepo.GetByIdAsync(id);
            if (schedule == null) return NotFound();
            _mapper.Map(dto, schedule);
            await _workScheduleRepo.UpdateAsync(schedule);
            return NoContent();
        }

        // DELETE: api/WorkSchedule/{id}
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
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
        //[Authorize(Roles = "Admin,Employee")]
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
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWorkScheduleCount()
        {
            var schedules = await _workScheduleRepo.GetAllAsync();
            return Ok(schedules.Count());
        }
        [HttpPost("bulk")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBulk([FromBody] BulkWorkScheduleCreateDTO request)
        {
            if (request?.Dtos == null || !request.Dtos.Any())
                return BadRequest("Danh sách lịch trống.");
            if (!request.StartDate.HasValue || !request.EndDate.HasValue)
                return BadRequest("Phải chọn ngày bắt đầu và ngày kết thúc.");
            if (request.EndDate < request.StartDate)
                return BadRequest("Ngày kết thúc phải sau ngày bắt đầu.");

            var created = new List<WorkScheduleDTO>();
            var skipped = new List<string>();

            var allSchedules = await _workScheduleRepo.GetAllAsync();
            var users = await _userRepo.GetAllAsync(); // load trước danh sách nhân viên

            foreach (var dto in request.Dtos)
            {
                for (var date = request.StartDate.Value.Date; date <= request.EndDate.Value.Date; date = date.AddDays(1))
                {
                    // Check trùng
                    var exists = allSchedules.Any(s => s.UserId == dto.UserId &&
                                                       s.WorkDate.Date == date &&
                                                       s.WorkShiftId == dto.WorkShiftId);
                    if (exists)
                    {
                        var name = users.FirstOrDefault(u => u.Id == dto.UserId)?.FullName ?? $"UserId {dto.UserId}";
                        skipped.Add($"{name} đã có lịch {date:dd/MM}");
                        continue;
                    }

                    // Tạo schedule mới
                    var newDto = new WorkScheduleCreateDTO
                    {
                        UserId = dto.UserId,
                        WorkShiftId = dto.WorkShiftId,
                        WorkDate = date,
                        Status = dto.Status
                    };

                    var schedule = _mapper.Map<WorkSchedule>(newDto);
                    await _workScheduleRepo.AddAsync(schedule);
                    created.Add(_mapper.Map<WorkScheduleDTO>(schedule));
                }
            }

            return Ok(new { created, skipped });
        }


    }
}

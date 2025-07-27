using Repository;

namespace WebAPI.Services
{
    public class WorkScheduleUpdateService : IWorkScheduleUpdateService
    {
        private readonly IWorkScheduleRepository _workScheduleRepo;
        private readonly IAttendanceRecordRepository _attendanceRepo;
        private readonly IWorkScheduleEvaluatorService _evaluator;

        public WorkScheduleUpdateService(IWorkScheduleRepository workScheduleRepo,
                                         IAttendanceRecordRepository attendanceRepo,
                                         IWorkScheduleEvaluatorService evaluator)
        {
            _workScheduleRepo = workScheduleRepo;
            _attendanceRepo = attendanceRepo;
            _evaluator = evaluator;
        }

        public async Task UpdateScheduleStatusAsync(int workScheduleId)
        {
            var schedule = await _workScheduleRepo.GetByIdAsync(workScheduleId);
            if (schedule == null) return;

            var records = await _attendanceRepo.GetByUserIdAsync(schedule.UserId);
            var checkIn = records.Where(a => a.WorkScheduleId == workScheduleId && a.Type == "CheckIn")
                                 .OrderBy(a => a.RecordTime)
                                 .FirstOrDefault();
            var checkOut = records.Where(a => a.WorkScheduleId == workScheduleId && a.Type == "CheckOut")
                                  .OrderByDescending(a => a.RecordTime)
                                  .FirstOrDefault();

            var (hoursWorked, status) = _evaluator.EvaluateStatus(schedule.WorkDate, schedule.WorkShift.StartTime, schedule.WorkShift.EndTime, checkIn?.RecordTime, checkOut?.RecordTime);
            schedule.Status = status;
            await _workScheduleRepo.UpdateAsync(schedule);
        }
    }
}

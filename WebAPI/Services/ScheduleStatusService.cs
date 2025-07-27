using BusinessObject.Models;
using Repository;

namespace WebAPI.Services
{
    public class ScheduleStatusService
    {
        private readonly IWorkScheduleRepository _workScheduleRepo;
        private readonly IEmailService _emailService;

        public ScheduleStatusService(IWorkScheduleRepository workScheduleRepo, IEmailService emailService )
        {
            _workScheduleRepo = workScheduleRepo;
            _emailService = emailService;
        }

        public async Task UpdateNotYetToAbsent()
        {
            var now = DateTime.Now;
            var schedules = await _workScheduleRepo.GetAllAsync();

            var toUpdate = new List<WorkSchedule>();

            foreach (var s in schedules)
            {
                var scheduleEndTime = s.WorkDate.Date.Add(s.WorkShift.EndTime).AddHours(1); 

                Console.WriteLine($"[DEBUG] ScheduleId={s.Id}, Status={s.Status}, Now={now}, End+1h={scheduleEndTime}");

                if (s.Status == "not yet" && now > s.WorkDate.Date.AddHours(1))
                //if (s.Status == "not yet" && now > scheduleEndTime)
                {
                    s.Status = "Absent";
                    await _workScheduleRepo.UpdateAsync(s);
                    toUpdate.Add(s);
                }
            }

            Console.WriteLine($"[Hangfire] Updated {toUpdate.Count} schedules to Absent at {DateTime.Now}");
        }

        public async Task SendReminderForLateCheckIn()
        {
            var now = DateTime.Now;
            var schedules = await _workScheduleRepo.GetAllAsync();

            Console.WriteLine($"[Hangfire] Checking schedules at {now}");

            var toNotify = schedules.Where(s =>
                s.Status == "not yet" &&
                s.User != null && // tránh null
                !string.IsNullOrEmpty(s.User.Email) && // tránh email rỗng
                now > s.WorkDate.Date.Add(s.WorkShift.StartTime).AddMinutes(5) &&
                now < s.WorkDate.Date.Add(s.WorkShift.StartTime).AddMinutes(15)
            ).ToList();

            //var toNotify = schedules.Where(s =>
            //s.Status == "not yet" &&
            //s.WorkDate.Date == DateTime.Today
            //).ToList();

            Console.WriteLine($"[DEBUG] Found {toNotify.Count} schedules to notify.");

            foreach (var schedule in toNotify)
            {
                Console.WriteLine($"[DEBUG] ScheduleId={schedule.Id}, UserId={schedule.UserId}, " +
                                  $"UserEmail={schedule.User?.Email}, " +
                                  $"Start={schedule.WorkDate.Date.Add(schedule.WorkShift.StartTime)}, " +
                                  $"Now={now}");

                try
                {
                    string subject = "Reminder: You haven’t checked in!";
                    string body = $"Hi {schedule.User.FullName},<br><br>" +
                                  $"You have a scheduled shift today ({schedule.WorkDate:dd/MM/yyyy}) from {schedule.WorkShift.StartTime} to {schedule.WorkShift.EndTime}, but you haven’t checked in yet.<br><br>" +
                                  "Please check in as soon as possible.";

                    await _emailService.SendEmailAsync(schedule.User.Email, subject, body);

                    Console.WriteLine($"[Hangfire] Sent reminder email to {schedule.User.Email} at {DateTime.Now}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to send email for ScheduleId={schedule.Id}: {ex.Message}");
                }
            }
        }


    }
}

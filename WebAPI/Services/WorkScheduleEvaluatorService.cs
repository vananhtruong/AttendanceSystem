namespace WebAPI.Services
{
    public class WorkScheduleEvaluatorService : IWorkScheduleEvaluatorService
    {
        public (decimal hoursWorked, string status) EvaluateStatus(
    DateTime workDate,
    TimeSpan shiftStart,
    TimeSpan shiftEnd,
    DateTime? checkIn,
    DateTime? checkOut)
        {
            var shiftStartDateTime = workDate.Date.Add(shiftStart);
            var shiftEndDateTime = workDate.Date.Add(shiftEnd);
            var shiftDuration = (decimal)(shiftEndDateTime - shiftStartDateTime).TotalHours;

            // Nếu chưa tới ngày làm => giữ "NotYet"
            if (workDate.Date > DateTime.Today)
                return (0, "NotYet");

            if (!checkIn.HasValue || !checkOut.HasValue)
                return (0, "Absent");

            // Giới hạn trong khung ca
            var effectiveIn = checkIn < shiftStartDateTime ? shiftStartDateTime : checkIn.Value;
            var effectiveOut = checkOut > shiftEndDateTime ? shiftEndDateTime : checkOut.Value;

            var workedHours = effectiveOut > effectiveIn
                ? (decimal)(effectiveOut - effectiveIn).TotalHours
                : 0;

            if (workedHours == 0)
                return (0, "Absent");

            // Completed: làm >=97% ca và đúng khung
            if (workedHours >= shiftDuration * 0.97m
                && checkIn <= shiftStartDateTime
                && checkOut >= shiftEndDateTime)
            {
                return (workedHours, "Completed");
            }

            return (workedHours, "Insufficient");
        }

    }

}

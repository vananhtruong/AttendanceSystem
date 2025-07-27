namespace WebAPI.Services
{
    public interface IWorkScheduleEvaluatorService
    {
        (decimal hoursWorked, string status) EvaluateStatus(
            DateTime workDate,
            TimeSpan shiftStart,
            TimeSpan shiftEnd,
            DateTime? checkIn,
            DateTime? checkOut);
    }
}

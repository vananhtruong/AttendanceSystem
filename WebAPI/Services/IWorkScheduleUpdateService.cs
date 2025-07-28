namespace WebAPI.Services
{
    public interface IWorkScheduleUpdateService
    {
        Task UpdateScheduleStatusAsync(int workScheduleId);
    }
}

namespace WebAPI.Services
{
    public interface IFaceRecognitionService
    {
        Task<object> RecognizeAndRecordAttendanceAsync(Stream imageStream);
        Task<bool> ProcessCheckInAsync(int userId, int workScheduleId, Stream imageStream);
        Task<bool> ProcessCheckOutAsync(int userId, int workScheduleId, Stream imageStream);
        Task<bool> UpdateWorkScheduleStatusAsync(int workScheduleId);
        Task RegisterUserAsync(int userId, Stream imageStream);
        Task<bool> HasFaceRegisteredAsync(int userId);
        Task RemoveFaceDataAsync(int userId);
        
        // Optional: Methods for saving images to file system
        Task<string> SaveImageToFileAsync(Stream imageStream, int userId);
        Task<bool> DeleteImageFileAsync(int userId);
    }
}

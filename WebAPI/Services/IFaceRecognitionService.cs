namespace WebAPI.Services
{
    public interface IFaceRecognitionService
    {
        Task<string> RecognizeAndRecordAttendanceAsync(Stream imageStream);
        Task RegisterUserAsync(int userId, Stream imageStream);
        Task<bool> HasFaceRegisteredAsync(int userId);
        Task RemoveFaceDataAsync(int userId);
        
        // Optional: Methods for saving images to file system
        Task<string> SaveImageToFileAsync(Stream imageStream, int userId);
        Task<bool> DeleteImageFileAsync(int userId);
    }
}

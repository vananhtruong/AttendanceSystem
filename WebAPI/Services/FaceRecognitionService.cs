using BusinessObject.Models;
using System.Text.Json;
using Repository;

namespace WebAPI.Services
{
    public class FaceRecognitionService : IFaceRecognitionService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAttendanceRecordRepository _attendanceRepository;

        public FaceRecognitionService(IUserRepository userRepository, IAttendanceRecordRepository attendanceRepository)
        {
            _userRepository = userRepository;
            _attendanceRepository = attendanceRepository;
        }

        public async Task<string> RecognizeAndRecordAttendanceAsync(Stream imageStream)
        {
            // Placeholder implementation
            return "Face recognition not implemented yet";
        }

        public async Task RegisterUserAsync(int userId, Stream imageStream)
        {
            if (userId <= 0)
                throw new ArgumentException("Valid UserId is required");

            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null)
                throw new InvalidOperationException("User not found");

            // Placeholder implementation - just mark as registered
            existingUser.FaceDescriptorJson = "{\"registered\": true}";
            await _userRepository.UpdateAsync(existingUser);
        }

        public async Task<bool> HasFaceRegisteredAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.FaceDescriptorJson != null;
        }

        public async Task RemoveFaceDataAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.FaceDescriptorJson = null;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task<string> SaveImageToFileAsync(Stream imageStream, int userId)
        {
            // Placeholder implementation
            return $"face_{userId}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
        }

        public async Task<bool> DeleteImageFileAsync(int userId)
        {
            // Placeholder implementation
            return true;
        }
    }
}

using BusinessObject.Models;
using System.Text.Json;
using DlibDotNet;
using DlibDotNet.Dnn;
using System.Drawing;
using Repository;
using DlibDotNet.Extensions;

namespace WebAPI.Services
{
    public class FaceRecognitionService : IFaceRecognitionService, IDisposable
    {
        private readonly ShapePredictor _shapePredictor;
        private readonly LossMetric _faceRecognitionModel;
        private readonly FrontalFaceDetector _faceDetector;
        private readonly IUserRepository _userRepository;
        private readonly IAttendanceRecordRepository _attendanceRepository;
        private readonly IWorkScheduleRepository _workScheduleRepository;
        private readonly IWebHostEnvironment _environment;
        private bool _disposed;

        public FaceRecognitionService(
            IWebHostEnvironment env, 
            IUserRepository userRepository, 
            IAttendanceRecordRepository attendanceRepository,
            IWorkScheduleRepository workScheduleRepository)
        {
            var modelDir = Path.Combine(env.ContentRootPath, "Models");
            _shapePredictor = ShapePredictor.Deserialize(Path.Combine(modelDir, "shape_predictor_68_face_landmarks.dat"));
            _faceRecognitionModel = LossMetric.Deserialize(Path.Combine(modelDir, "dlib_face_recognition_resnet_model_v1.dat"));
            _faceDetector = Dlib.GetFrontalFaceDetector();
            _userRepository = userRepository;
            _attendanceRepository = attendanceRepository;
            _workScheduleRepository = workScheduleRepository;
            _environment = env;
        }

        public async Task<object> RecognizeAndRecordAttendanceAsync(Stream imageStream)
        {
            try
            {
                Console.WriteLine("=== Face Recognition Process Started ===");
                
                if (imageStream == null || (imageStream.CanSeek && imageStream.Length == 0))
                    throw new InvalidOperationException("Invalid image data");

                if (imageStream.CanSeek)
                    imageStream.Seek(0, SeekOrigin.Begin);

                using var bitmap = LoadAndPreprocessImage(imageStream);
                using var img = bitmap.ToArray2D<RgbPixel>();

                Console.WriteLine("Detecting faces in image...");
                var faces = _faceDetector.Operator(img);
                Console.WriteLine($"Found {faces.Length} face(s) in image");
                
                if (faces.Length == 0)
                {
                    Console.WriteLine("ERROR: No face detected in the image");
                    throw new InvalidOperationException("No face detected in the image");
                }

                // Use the largest face for recognition
                var largestFace = faces.OrderByDescending(f => f.Width * f.Height).First();
                var faceRect = new DlibDotNet.Rectangle(largestFace.Left, largestFace.Top, largestFace.Right, largestFace.Bottom);
                Console.WriteLine($"Using largest face: {largestFace.Width}x{largestFace.Height} at ({largestFace.Left},{largestFace.Top})");
                
                var recognitionResult = await RecognizeFaceAsync(img, faceRect);

                if (!recognitionResult.IsRecognized || recognitionResult.User == null)
                {
                    Console.WriteLine("ERROR: Face not recognized - no matching user found");
                    throw new InvalidOperationException("Face not recognized. Please register your face first.");
                }

                // Find the nearest work schedule for today
                var today = DateTime.Today;
                var nearestSchedule = await FindNearestWorkScheduleAsync(recognitionResult.User.Id, today);
                
                if (nearestSchedule == null)
                {
                    throw new InvalidOperationException("No work schedule found for today");
                }
                
                // Check current attendance status
                var attendanceStatus = await GetCurrentAttendanceStatusAsync(recognitionResult.User.Id, nearestSchedule.Id);
                
                return new
                {
                    user = new
                    {
                        id = recognitionResult.User.Id,
                        fullName = recognitionResult.User.FullName,
                        email = recognitionResult.User.Email
                    },
                    schedule = new
                    {
                        id = nearestSchedule.Id,
                        shiftName = nearestSchedule.WorkShift.Name,
                        shiftStart = nearestSchedule.WorkShift.StartTime.ToString(@"hh\:mm"),
                        shiftEnd = nearestSchedule.WorkShift.EndTime.ToString(@"hh\:mm"),
                        workDate = nearestSchedule.WorkDate,
                        attendanceStatus = attendanceStatus.CurrentStatus,
                        canCheckIn = attendanceStatus.CanCheckIn,
                        canCheckOut = attendanceStatus.CanCheckOut,
                        lastCheckIn = attendanceStatus.LastCheckIn,
                        lastCheckOut = attendanceStatus.LastCheckOut
                    }
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Face recognition failed: {ex.Message}");
            }
        }

        public async Task<bool> ProcessCheckInAsync(int userId, int workScheduleId, Stream imageStream)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new InvalidOperationException("User not found");

                var schedule = await _workScheduleRepository.GetByIdAsync(workScheduleId);
                if (schedule == null)
                    throw new InvalidOperationException("Work schedule not found");

                // Verify face matches the user
                if (!await VerifyFaceAsync(user, imageStream))
                {
                    throw new InvalidOperationException("Face verification failed");
                }

                // Check if already checked in for this schedule
                var existingCheckIn = await _attendanceRepository.GetByUserIdAndDateAsync(
                    userId, schedule.WorkDate, "CheckIn");
                
                if (existingCheckIn != null)
                    throw new InvalidOperationException("Already checked in for this shift");

                // Save attendance image
                var imagePath = await SaveImageToFileAsync(imageStream, userId);

                // Create check-in record
                var checkInRecord = new AttendanceRecord
                {
                    UserId = userId,
                    WorkScheduleId = workScheduleId,
                    RecordTime = DateTime.Now,
                    Type = "CheckIn",
                    Status = DetermineCheckInStatus(schedule.WorkDate, schedule.WorkShift.StartTime),
                    HoursWorked = 0,
                    OvertimeHours = 0
                };

                await _attendanceRepository.AddAsync(checkInRecord);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Check-in failed: {ex.Message}");
            }
        }

        public async Task<bool> ProcessCheckOutAsync(int userId, int workScheduleId, Stream imageStream)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new InvalidOperationException("User not found");

                var schedule = await _workScheduleRepository.GetByIdAsync(workScheduleId);
                if (schedule == null)
                    throw new InvalidOperationException("Work schedule not found");

                // Verify face matches the user
                if (!await VerifyFaceAsync(user, imageStream))
                {
                    throw new InvalidOperationException("Face verification failed");
                }

                // Check if already checked out for this schedule
                var existingCheckOut = await _attendanceRepository.GetByUserIdAndDateAsync(
                    userId, schedule.WorkDate, "CheckOut");
                
                if (existingCheckOut != null)
                    throw new InvalidOperationException("Already checked out for this shift");

                // Check if checked in first
                var checkInRecord = await _attendanceRepository.GetByUserIdAndDateAsync(
                    userId, schedule.WorkDate, "CheckIn");
                
                if (checkInRecord == null)
                    throw new InvalidOperationException("Must check in before checking out");

                // Save attendance image
                var imagePath = await SaveImageToFileAsync(imageStream, userId);

                // Create check-out record
                var checkOutRecord = new AttendanceRecord
                {
                    UserId = userId,
                    WorkScheduleId = workScheduleId,
                    RecordTime = DateTime.Now,
                    Type = "CheckOut",
                    Status = DetermineCheckOutStatus(schedule.WorkDate, schedule.WorkShift.EndTime),
                    HoursWorked = CalculateHoursWorked(checkInRecord.RecordTime, DateTime.Now),
                    OvertimeHours = CalculateOvertimeHours(checkInRecord.RecordTime, DateTime.Now, schedule.WorkShift)
                };

                await _attendanceRepository.AddAsync(checkOutRecord);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Check-out failed: {ex.Message}");
            }
        }

        public async Task<bool> UpdateWorkScheduleStatusAsync(int workScheduleId)
        {
            try
            {
                var schedule = await _workScheduleRepository.GetByIdAsync(workScheduleId);
                if (schedule == null)
                    return false;

                var attendanceRecords = await _attendanceRepository.GetByUserIdAsync(schedule.UserId);
                var checkIn = attendanceRecords
                    .Where(a => a.WorkScheduleId == workScheduleId && a.Type == "CheckIn")
                    .OrderBy(a => a.RecordTime)
                    .FirstOrDefault();
                var checkOut = attendanceRecords
                    .Where(a => a.WorkScheduleId == workScheduleId && a.Type == "CheckOut")
                    .OrderByDescending(a => a.RecordTime)
                    .FirstOrDefault();

                // Calculate new status using evaluator service
                var evaluator = new WorkScheduleEvaluatorService();
                var (hoursWorked, newStatus) = evaluator.EvaluateStatus(
                    schedule.WorkDate,
                    schedule.WorkShift.StartTime,
                    schedule.WorkShift.EndTime,
                    checkIn?.RecordTime,
                    checkOut?.RecordTime
                );

                schedule.Status = newStatus;
                await _workScheduleRepository.UpdateAsync(schedule);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update work schedule status: {ex.Message}");
            }
        }

        public async Task RegisterUserAsync(int userId, Stream imageStream)
        {
            if (userId <= 0)
                throw new ArgumentException("Valid UserId is required");

            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null)
                throw new InvalidOperationException("User not found");

            if (imageStream == null || (imageStream.CanSeek && imageStream.Length == 0))
                throw new ArgumentException("Invalid image data");

            if (imageStream.CanSeek)
                imageStream.Seek(0, SeekOrigin.Begin);

            using var bitmap = LoadAndPreprocessImage(imageStream);
            using var img = bitmap.ToArray2D<RgbPixel>();

            var faces = _faceDetector.Operator(img);
            if (faces.Length == 0)
                throw new InvalidOperationException("No face detected in the image");

            var largestFace = faces.OrderByDescending(f => f.Width * f.Height).First();
            var faceRect = new DlibDotNet.Rectangle(largestFace.Left, largestFace.Top, largestFace.Right, largestFace.Bottom);
            var descriptor = GetFaceDescriptor(img, faceRect);
            var embeddingJson = JsonSerializer.Serialize(descriptor);

            // Save processed image to file system
            var imagePath = await SaveProcessedImageToFileAsync(bitmap, userId);
            
            // Create face descriptor data
            var faceData = new
            {
                registered = true,
                imagePath = imagePath,
                registeredAt = DateTime.UtcNow,
                userId = userId,
                descriptor = embeddingJson
            };
            
            existingUser.FaceDescriptorJson = JsonSerializer.Serialize(faceData);
            await _userRepository.UpdateAsync(existingUser);
        }

        public async Task<bool> HasFaceRegisteredAsync(int userId)
        {
            if (userId <= 0)
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            return user != null && !string.IsNullOrEmpty(user.FaceDescriptorJson);
        }

        public async Task RemoveFaceDataAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Valid UserId is required");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                // Delete image file if exists
                await DeleteImageFileAsync(userId);
                
                user.FaceDescriptorJson = null;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task<string> SaveImageToFileAsync(Stream imageStream, int userId)
        {
            // Create uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "faces");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Generate unique filename
            var fileName = $"face_{userId}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save image to file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            return fileName;
        }

        private async Task<string> SaveProcessedImageToFileAsync(Bitmap bitmap, int userId)
        {
            // Create uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "faces");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Generate unique filename
            var fileName = $"face_{userId}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save processed bitmap to file
            bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

            return fileName;
        }

        public async Task<bool> DeleteImageFileAsync(int userId)
        {
            try
            {
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "faces");
                var files = Directory.GetFiles(uploadsDir, $"face_{userId}_*.jpg");
                
                foreach (var file in files)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Bitmap LoadAndPreprocessImage(Stream imageStream)
        {
            try
            {
                using var original = new Bitmap(imageStream);
                Console.WriteLine($"Image preprocessing: Original size = {original.Width}x{original.Height}");
                
                var bitmap = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                using (var g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(original, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
                }

                if (Array.IndexOf(bitmap.PropertyIdList, 274) > -1)
                {
                    var orientation = (int)bitmap.GetPropertyItem(274).Value[0];
                    Console.WriteLine($"Image preprocessing: Orientation = {orientation}");
                    switch (orientation)
                    {
                        case 6: bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                        case 8: bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
                        case 3: bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                    }
                }

                const int maxDimension = 1500;
                if (bitmap.Width > maxDimension || bitmap.Height > maxDimension)
                {
                    var ratio = Math.Min((double)maxDimension / bitmap.Width, (double)maxDimension / bitmap.Height);
                    var newWidth = (int)(bitmap.Width * ratio);
                    var newHeight = (int)(bitmap.Height * ratio);

                    Console.WriteLine($"Image preprocessing: Resizing to {newWidth}x{newHeight} (ratio = {ratio})");

                    var resized = new Bitmap(newWidth, newHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (var g = Graphics.FromImage(resized))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(bitmap, 0, 0, newWidth, newHeight);
                    }
                    bitmap.Dispose();
                    return resized;
                }

                Console.WriteLine($"Image preprocessing: Final size = {bitmap.Width}x{bitmap.Height}");
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image preprocessing error: {ex.Message}");
                throw new Exception("Failed to load image: " + ex.Message);
            }
        }

        private float[] GetFaceDescriptor(Array2D<RgbPixel> img, DlibDotNet.Rectangle face)
        {
            var shape = _shapePredictor.Detect(img, face);
            using var faceChip = Dlib.ExtractImageChip<RgbPixel>(
                img,
                Dlib.GetFaceChipDetails(shape, 150, 0.25));

            using var matrix = new Matrix<RgbPixel>(faceChip);
            using var descriptor = _faceRecognitionModel.Operator(matrix);

            return descriptor.First().ToArray();
        }

        private async Task<(bool IsRecognized, User User)> RecognizeFaceAsync(Array2D<RgbPixel> img, DlibDotNet.Rectangle face)
        {
            const double recognitionThreshold = 0.55;
            var inputVec = GetFaceDescriptor(img, face);
            var users = await _userRepository.GetAllAsync();

            Console.WriteLine($"Total users in database: {users.Count}");

            double minDistance = double.MaxValue;
            User matchedUser = null;
            int usersWithFaceData = 0;
            int validFaceDescriptors = 0;

            foreach (var user in users)
            {
                try
                {
                    if (string.IsNullOrEmpty(user.FaceDescriptorJson))
                    {
                        Console.WriteLine($"User {user.Id} ({user.FullName}): No face data");
                        continue;
                    }

                    usersWithFaceData++;
                    Console.WriteLine($"User {user.Id} ({user.FullName}): Has face data");

                    var faceData = JsonSerializer.Deserialize<JsonElement>(user.FaceDescriptorJson);
                    if (!faceData.TryGetProperty("descriptor", out var descriptorElement))
                    {
                        Console.WriteLine($"User {user.Id}: No descriptor property");
                        continue;
                    }

                    var dbVec = JsonSerializer.Deserialize<float[]>(descriptorElement.GetString());
                    if (dbVec == null || dbVec.Length != inputVec.Length)
                    {
                        Console.WriteLine($"User {user.Id}: Invalid descriptor (length: {dbVec?.Length ?? 0}, expected: {inputVec.Length})");
                        continue;
                    }

                    validFaceDescriptors++;
                    var distance = EuclideanDistance(inputVec, dbVec);
                    Console.WriteLine($"User {user.Id} ({user.FullName}): Distance = {distance:F4}");

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        matchedUser = user;
                        Console.WriteLine($"User {user.Id} ({user.FullName}): New best match (distance = {distance:F4})");
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"User {user.Id}: JSON parsing error - {ex.Message}");
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"User {user.Id}: Unexpected error - {ex.Message}");
                    continue;
                }
            }

            // Log thông tin debug
            Console.WriteLine($"Face Recognition Debug Summary:");
            Console.WriteLine($"- Total users: {users.Count}");
            Console.WriteLine($"- Users with face data: {usersWithFaceData}");
            Console.WriteLine($"- Valid face descriptors: {validFaceDescriptors}");
            Console.WriteLine($"- Min distance found: {minDistance:F4}");
            Console.WriteLine($"- Recognition threshold: {recognitionThreshold}");
            Console.WriteLine($"- Is recognized: {minDistance < recognitionThreshold}");

            // Chỉ trả về user nếu khoảng cách nhỏ hơn threshold
            // Nếu không có user nào đạt threshold, trả về null
            if (minDistance >= recognitionThreshold)
            {
                Console.WriteLine($"- Result: No face recognized (distance {minDistance:F4} >= threshold {recognitionThreshold})");
                return (false, null);
            }

            Console.WriteLine($"- Result: Face recognized as {matchedUser?.FullName} (distance {minDistance:F4})");
            return (true, matchedUser);
        }

        private async Task<bool> VerifyFaceAsync(User user, Stream imageStream)
        {
            try
            {
                if (string.IsNullOrEmpty(user.FaceDescriptorJson))
                    return false;

                if (imageStream.CanSeek)
                    imageStream.Seek(0, SeekOrigin.Begin);

                using var bitmap = LoadAndPreprocessImage(imageStream);
                using var img = bitmap.ToArray2D<RgbPixel>();

                var faces = _faceDetector.Operator(img);
                if (faces.Length == 0)
                    return false;

                var largestFace = faces.OrderByDescending(f => f.Width * f.Height).First();
                var faceRect = new DlibDotNet.Rectangle(largestFace.Left, largestFace.Top, largestFace.Right, largestFace.Bottom);
                var recognitionResult = await RecognizeFaceAsync(img, faceRect);

                return recognitionResult.IsRecognized && recognitionResult.User?.Id == user.Id;
            }
            catch
            {
                return false;
            }
        }

        // Helper methods for work schedule and attendance
        private async Task<WorkSchedule> FindNearestWorkScheduleAsync(int userId, DateTime date)
        {
            var schedules = await _workScheduleRepository.GetByUserIdAsync(userId);
            
            // First, try to find today's schedule
            var todaySchedule = schedules.FirstOrDefault(s => s.WorkDate.Date == date.Date);
            if (todaySchedule != null)
                return todaySchedule;

            // If no schedule for today, find the nearest one (within 7 days)
            var nearestSchedule = schedules
                .Where(s => s.WorkDate.Date >= date.AddDays(-3) && s.WorkDate.Date <= date.AddDays(3))
                .OrderBy(s => Math.Abs((s.WorkDate.Date - date.Date).Days))
                .FirstOrDefault();

            return nearestSchedule;
        }

        private async Task<AttendanceStatusInfo> GetCurrentAttendanceStatusAsync(int userId, int workScheduleId)
        {
            var attendanceRecords = await _attendanceRepository.GetByUserIdAsync(userId);
            var scheduleRecords = attendanceRecords.Where(a => a.WorkScheduleId == workScheduleId).ToList();

            var checkIn = scheduleRecords.FirstOrDefault(a => a.Type == "CheckIn");
            var checkOut = scheduleRecords.FirstOrDefault(a => a.Type == "CheckOut");

            var canCheckIn = checkIn == null;
            var canCheckOut = checkIn != null && checkOut == null;

            string currentStatus;
            if (checkIn == null)
                currentStatus = "Not Checked In";
            else if (checkOut == null)
                currentStatus = "Checked In";
            else
                currentStatus = "Completed";

            return new AttendanceStatusInfo
            {
                CurrentStatus = currentStatus,
                CanCheckIn = canCheckIn,
                CanCheckOut = canCheckOut,
                LastCheckIn = checkIn?.RecordTime,
                LastCheckOut = checkOut?.RecordTime
            };
        }

        private string DetermineCheckInStatus(DateTime workDate, TimeSpan shiftStart)
        {
            var now = DateTime.Now;
            var shiftStartTime = workDate.Date.Add(shiftStart);
            
            // Allow 15 minutes grace period
            var gracePeriod = shiftStartTime.AddMinutes(15);
            
            if (now <= gracePeriod)
                return "OnTime";
            else
                return "Late";
        }

        private string DetermineCheckOutStatus(DateTime workDate, TimeSpan shiftEnd)
        {
            var now = DateTime.Now;
            var shiftEndTime = workDate.Date.Add(shiftEnd);
            
            if (now >= shiftEndTime)
                return "OnTime";
            else
                return "EarlyLeave";
        }

        private decimal CalculateHoursWorked(DateTime checkInTime, DateTime checkOutTime)
        {
            var duration = checkOutTime - checkInTime;
            return Math.Round((decimal)duration.TotalHours, 2);
        }

        private decimal CalculateOvertimeHours(DateTime checkInTime, DateTime checkOutTime, WorkShift shift)
        {
            var totalHours = CalculateHoursWorked(checkInTime, checkOutTime);
            var shiftDuration = (decimal)(shift.EndTime - shift.StartTime).TotalHours;
            
            var overtime = totalHours - shiftDuration;
            return Math.Max(0, Math.Round(overtime, 2));
        }

        private float EuclideanDistance(float[] a, float[] b)
        {
            float sum = 0f;
            for (int i = 0; i < a.Length; i++)
                sum += (a[i] - b[i]) * (a[i] - b[i]);
            return (float)Math.Sqrt(sum);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _shapePredictor?.Dispose();
                _faceRecognitionModel?.Dispose();
                _faceDetector?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }

    public class AttendanceStatusInfo
    {
        public string CurrentStatus { get; set; }
        public bool CanCheckIn { get; set; }
        public bool CanCheckOut { get; set; }
        public DateTime? LastCheckIn { get; set; }
        public DateTime? LastCheckOut { get; set; }
    }
}

// Extension method chuyển Bitmap sang Array2D<RgbPixel> cho DlibDotNet
public static class BitmapExtensions
{
    public static Array2D<RgbPixel> ToArray2D(this Bitmap bitmap)
    {
        var array = new Array2D<RgbPixel>(bitmap.Height, bitmap.Width);
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                array[y][x] = new RgbPixel { Red = color.R, Green = color.G, Blue = color.B };
            }
        }
        return array;
    }
}


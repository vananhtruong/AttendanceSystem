//using BusinessObject.Models;
//using System.Text.Json;
//using DlibDotNet;
//using DlibDotNet.Dnn;
//using System.Drawing;
//using Repository;
//using DlibDotNet.Extensions;

//namespace WebAPI.Services
//{
//    public class FaceRecognitionService : IFaceRecognitionService, IDisposable
//    {
//        private readonly ShapePredictor _shapePredictor;
//        private readonly LossMetric _faceRecognitionModel;
//        private readonly FrontalFaceDetector _faceDetector;
//        private readonly IUserRepository _userRepository;
//        private readonly IAttendanceRecordRepository _attendanceRepository;
//        private bool _disposed;

//        public FaceRecognitionService(IWebHostEnvironment env, IUserRepository userRepository, IAttendanceRecordRepository attendanceRepository)
//        {
//            var modelDir = Path.Combine(env.ContentRootPath, "Models");
//            _shapePredictor = ShapePredictor.Deserialize(Path.Combine(modelDir, "shape_predictor_68_face_landmarks.dat"));
//            _faceRecognitionModel = LossMetric.Deserialize(Path.Combine(modelDir, "dlib_face_recognition_resnet_model_v1.dat"));
//            _faceDetector = Dlib.GetFrontalFaceDetector();
//            _userRepository = userRepository;
//            _attendanceRepository = attendanceRepository;
//        }

//        public async Task<string> RecognizeAndRecordAttendanceAsync(Stream imageStream)
//        {
//            try
//            {
//                if (imageStream == null || (imageStream.CanSeek && imageStream.Length == 0))
//                    return "Invalid image data";

//                if (imageStream.CanSeek)
//                    imageStream.Seek(0, SeekOrigin.Begin);

//                using var bitmap = LoadAndPreprocessImage(imageStream);
//                using var img = bitmap.ToArray2D<RgbPixel>();

//                var faces = _faceDetector.Operator(img);
//                if (faces.Length == 0)
//                {
//                    return "No face detected";
//                }

//                foreach (var face in faces)
//                {
//                    var faceRect = new DlibDotNet.Rectangle(face.Left, face.Top, face.Right, face.Bottom);
//                    var recognitionResult = await RecognizeFaceAsync(img, faceRect);

//                    if (recognitionResult.IsRecognized && recognitionResult.User != null)
//                    {
//                        await RecordAttendance(recognitionResult.User);
//                        return $"Recognized: {recognitionResult.User.FullName}";
//                    }
//                }

//                return "Unknown face (not in database)";
//            }
//            catch (Exception ex)
//            {
//                return $"Error: {ex.Message}";
//            }
//        }

//        public async Task RegisterUserAsync(int userId, Stream imageStream)
//        {
//            if (userId <= 0)
//                throw new ArgumentException("Valid UserId is required");

//            var existingUser = await _userRepository.GetByIdAsync(userId);
//            if (existingUser == null)
//                throw new InvalidOperationException("User not found");

//            if (imageStream == null || (imageStream.CanSeek && imageStream.Length == 0))
//                throw new ArgumentException("Invalid image data");

//            if (imageStream.CanSeek)
//                imageStream.Seek(0, SeekOrigin.Begin);

//            using var bitmap = LoadAndPreprocessImage(imageStream);
//            using var img = bitmap.ToArray2D<RgbPixel>();

//            var faces = _faceDetector.Operator(img);
//            if (faces.Length == 0)
//                throw new InvalidOperationException("No face detected in the image");

//            var largestFace = faces.OrderByDescending(f => f.Width * f.Height).First();
//            var faceRect = new DlibDotNet.Rectangle(largestFace.Left, largestFace.Top, largestFace.Right, largestFace.Bottom);
//            var descriptor = GetFaceDescriptor(img, faceRect);
//            var embeddingJson = JsonSerializer.Serialize(descriptor);

//            // Update existing user with face descriptor
//            existingUser.FaceDescriptorJson = embeddingJson;
//            await _userRepository.UpdateAsync(existingUser);
//        }

//        public async Task<bool> HasFaceRegisteredAsync(int userId)
//        {
//            if (userId <= 0)
//                return false;

//            var user = await _userRepository.GetByIdAsync(userId);
//            return user != null && !string.IsNullOrEmpty(user.FaceDescriptorJson);
//        }

//        public async Task RemoveFaceDataAsync(int userId)
//        {
//            if (userId <= 0)
//                throw new ArgumentException("Valid UserId is required");

//            var user = await _userRepository.GetByIdAsync(userId);
//            if (user == null)
//                throw new InvalidOperationException("User not found");

//            user.FaceDescriptorJson = null;
//            await _userRepository.UpdateAsync(user);
//        }

//        private Bitmap LoadAndPreprocessImage(Stream imageStream)
//        {
//            try
//            {
//                using var original = new Bitmap(imageStream);
//                var bitmap = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

//                using (var g = Graphics.FromImage(bitmap))
//                {
//                    g.DrawImage(original, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
//                }

//                if (Array.IndexOf(bitmap.PropertyIdList, 274) > -1)
//                {
//                    var orientation = (int)bitmap.GetPropertyItem(274).Value[0];
//                    switch (orientation)
//                    {
//                        case 6: bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
//                        case 8: bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
//                        case 3: bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
//                    }
//                }

//                const int maxDimension = 1500;
//                if (bitmap.Width > maxDimension || bitmap.Height > maxDimension)
//                {
//                    var ratio = Math.Min((double)maxDimension / bitmap.Width, (double)maxDimension / bitmap.Height);
//                    var newWidth = (int)(bitmap.Width * ratio);
//                    var newHeight = (int)(bitmap.Height * ratio);

//                    var resized = new Bitmap(newWidth, newHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
//                    using (var g = Graphics.FromImage(resized))
//                    {
//                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
//                        g.DrawImage(bitmap, 0, 0, newWidth, newHeight);
//                    }
//                    bitmap.Dispose();
//                    return resized;
//                }

//                return bitmap;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Failed to load image: " + ex.Message);
//            }
//        }

//        private float[] GetFaceDescriptor(Array2D<RgbPixel> img, DlibDotNet.Rectangle face)
//        {
//            var shape = _shapePredictor.Detect(img, face);
//            using var faceChip = Dlib.ExtractImageChip<RgbPixel>(
//                img,
//                Dlib.GetFaceChipDetails(shape, 150, 0.25));

//            using var matrix = new Matrix<RgbPixel>(faceChip);
//            using var descriptor = _faceRecognitionModel.Operator(matrix);

//            return descriptor.First().ToArray();
//        }

//        private async Task<(bool IsRecognized, User User)> RecognizeFaceAsync(Array2D<RgbPixel> img, DlibDotNet.Rectangle face)
//        {
//            const double recognitionThreshold = 0.55;
//            var inputVec = GetFaceDescriptor(img, face);
//            var users = await _userRepository.GetAllAsync();

//            double minDistance = double.MaxValue;
//            User matchedUser = null;

//            foreach (var user in users)
//            {
//                try
//                {
//                    var dbVec = JsonSerializer.Deserialize<float[]>(user.FaceDescriptorJson);
//                    if (dbVec == null || dbVec.Length != inputVec.Length) continue;

//                    var distance = EuclideanDistance(inputVec, dbVec);

//                    if (distance < minDistance)
//                    {
//                        minDistance = distance;
//                        matchedUser = user;
//                    }
//                }
//                catch (JsonException) { continue; }
//            }

//            return (minDistance < recognitionThreshold, matchedUser);
//        }

//        private async Task RecordAttendance(User user)
//        {
//            var today = DateTime.Today;
//            var existingAttendance = await _attendanceRepository.GetByUserIdAndDateAsync(user.Id, today);
            
//            if (existingAttendance != null)
//            {
//                // Update checkout time if already checked in
//                existingAttendance.CheckOutTime = DateTime.Now;
//                await _attendanceRepository.UpdateAsync(existingAttendance);
//                return;
//            }

//            var attendance = new AttendanceRecord
//            {
//                UserId = user.Id,
//                CheckInTime = DateTime.Now,
//                Date = today,
//                Status = "Present"
//            };
//            await _attendanceRepository.AddAsync(attendance);
//        }

//        private float EuclideanDistance(float[] a, float[] b)
//        {
//            float sum = 0f;
//            for (int i = 0; i < a.Length; i++)
//                sum += (a[i] - b[i]) * (a[i] - b[i]);
//            return (float)Math.Sqrt(sum);
//        }

//        public async Task<string> SaveImageToFileAsync(Stream imageStream, int userId)
//        {
//            // Implementation for saving image to file system
//            // This is optional and can be implemented later if needed
//            throw new NotImplementedException("Image file saving not implemented");
//        }

//        public async Task<bool> DeleteImageFileAsync(int userId)
//        {
//            // Implementation for deleting image file
//            // This is optional and can be implemented later if needed
//            throw new NotImplementedException("Image file deletion not implemented");
//        }

//        public void Dispose()
//        {
//            if (!_disposed)
//            {
//                _shapePredictor?.Dispose();
//                _faceRecognitionModel?.Dispose();
//                _faceDetector?.Dispose();
//                _disposed = true;
//            }
//            GC.SuppressFinalize(this);
//        }
//    }
//}

//// Extension method chuyển Bitmap sang Array2D<RgbPixel> cho DlibDotNet
//public static class BitmapExtensions
//{
//    public static Array2D<RgbPixel> ToArray2D(this Bitmap bitmap)
//    {
//        var array = new Array2D<RgbPixel>(bitmap.Height, bitmap.Width);
//        for (int y = 0; y < bitmap.Height; y++)
//        {
//            for (int x = 0; x < bitmap.Width; x++)
//            {
//                var color = bitmap.GetPixel(x, y);
//                array[y][x] = new RgbPixel { Red = color.R, Green = color.G, Blue = color.B };
//            }
//        }
//        return array;
//    }
//}

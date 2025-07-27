using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Services;
using BusinessObject.DTOs;
using Repository;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class FaceAttendanceController : ControllerBase
    {
        private readonly IFaceRecognitionService _faceRecognitionService;
        private readonly ILogger<FaceAttendanceController> _logger;
        private readonly IWorkScheduleRepository _workScheduleRepo;
        private readonly IAttendanceRecordRepository _attendanceRecordRepo;
        private readonly IWorkScheduleEvaluatorService _evaluator;

        public FaceAttendanceController(
            IFaceRecognitionService faceRecognitionService,
            ILogger<FaceAttendanceController> logger,
            IWorkScheduleRepository workScheduleRepo,
            IAttendanceRecordRepository attendanceRecordRepo,
            IWorkScheduleEvaluatorService evaluator)
        {
            _faceRecognitionService = faceRecognitionService;
            _logger = logger;
            _workScheduleRepo = workScheduleRepo;
            _attendanceRecordRepo = attendanceRecordRepo;
            _evaluator = evaluator;
        }

        /// <summary>
        /// Check-in using face recognition
        /// </summary>
        /// <param name="request">Face image data</param>
        /// <returns>Recognition result</returns>
        [HttpPost("checkin")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto>> CheckIn([FromForm] FaceImageRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponseDto.ErrorResult("Invalid input", ModelState));

                using var stream = request.FaceImage.OpenReadStream();
                var result = await _faceRecognitionService.RecognizeAndRecordAttendanceAsync(stream);

                return Ok(ApiResponseDto.SuccessResult(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-in");
                return StatusCode(500,
                    ApiResponseDto.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Register face data for existing user
        /// </summary>
        /// <param name="request">Face registration data</param>
        /// <returns>Registration result</returns>
        [HttpPost("register-face")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto>> RegisterFace([FromForm] RegisterUserRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponseDto.ErrorResult("Validation error", ModelState));

                using var stream = request.FaceImage.OpenReadStream();
                await _faceRecognitionService.RegisterUserAsync(
                    request.UserId,
                    stream);

                return Ok(ApiResponseDto.SuccessResult("Face data registered successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during face registration");
                return StatusCode(500,
                    ApiResponseDto.ErrorResult("Face registration failed"));
            }
        }

        /// <summary>
        /// Register face data for current user
        /// </summary>
        /// <param name="request">Face image data</param>
        /// <returns>Registration result</returns>
        [HttpPost("register-my-face")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto>> RegisterMyFace([FromForm] FaceImageRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponseDto.ErrorResult("Validation error", ModelState));

                // Get current user ID from token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized(ApiResponseDto.ErrorResult("Invalid user token"));

                using var stream = request.FaceImage.OpenReadStream();
                await _faceRecognitionService.RegisterUserAsync(userId, stream);

                return Ok(ApiResponseDto.SuccessResult("Your face data registered successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during face registration");
                return StatusCode(500,
                    ApiResponseDto.ErrorResult("Face registration failed"));
            }
        }

        /// <summary>
        /// Check if current user has registered face data
        /// </summary>
        /// <returns>Face registration status</returns>
        [HttpGet("face-status")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponseDto>> GetFaceStatus()
        {
            try
            {
                // Get current user ID from token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized(ApiResponseDto.ErrorResult("Invalid user token"));

                var hasFaceRegistered = await _faceRecognitionService.HasFaceRegisteredAsync(userId);
                
                return Ok(ApiResponseDto.SuccessResult(new { 
                    HasFaceRegistered = hasFaceRegistered,
                    Message = hasFaceRegistered ? "Face data is registered" : "Face data is not registered"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking face status");
                return StatusCode(500,
                    ApiResponseDto.ErrorResult("Failed to check face status"));
            }
        }

        /// <summary>
        /// Remove face data for current user
        /// </summary>
        /// <returns>Removal result</returns>
        [HttpDelete("remove-my-face")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto>> RemoveMyFace()
        {
            try
            {
                // Get current user ID from token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized(ApiResponseDto.ErrorResult("Invalid user token"));

                await _faceRecognitionService.RemoveFaceDataAsync(userId);

                return Ok(ApiResponseDto.SuccessResult("Face data removed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing face data");
                return StatusCode(500,
                    ApiResponseDto.ErrorResult("Failed to remove face data"));
            }
        }

        /// <summary>
        /// Remove face data for specific user (Admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Removal result</returns>
        [HttpDelete("remove-face/{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto>> RemoveUserFace(int userId)
        {
            try
            {
                await _faceRecognitionService.RemoveFaceDataAsync(userId);

                return Ok(ApiResponseDto.SuccessResult($"Face data removed successfully for user {userId}"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseDto.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing face data for user {UserId}", userId);
                return StatusCode(500,
                    ApiResponseDto.ErrorResult("Failed to remove face data"));
            }
        }
        /// <summary>
        /// Cập nhật trạng thái WorkSchedule dựa trên giờ check-in/check-out
        /// </summary>
        [HttpPost("update-schedule-status/{scheduleId}")]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto>> UpdateWorkScheduleStatus(int scheduleId)
        {
            try
            {
                var schedule = await _workScheduleRepo.GetByIdAsync(scheduleId);
                if (schedule == null)
                    return NotFound(ApiResponseDto.ErrorResult("WorkSchedule not found"));

                var attendanceRecords = await _attendanceRecordRepo.GetByUserIdAsync(schedule.UserId);
                var checkIn = attendanceRecords
                    .Where(a => a.WorkScheduleId == schedule.Id && a.Type == "CheckIn")
                    .OrderBy(a => a.RecordTime)
                    .FirstOrDefault();
                var checkOut = attendanceRecords
                    .Where(a => a.WorkScheduleId == schedule.Id && a.Type == "CheckOut")
                    .OrderByDescending(a => a.RecordTime)
                    .FirstOrDefault();

                // Tính toán trạng thái mới
                var (hoursWorked, newStatus) = _evaluator.EvaluateStatus(
                    schedule.WorkDate,
                    schedule.WorkShift.StartTime,
                    schedule.WorkShift.EndTime,
                    checkIn?.RecordTime,
                    checkOut?.RecordTime
                );

                schedule.Status = newStatus;
                await _workScheduleRepo.UpdateAsync(schedule);

                return Ok(ApiResponseDto.SuccessResult(new
                {
                    schedule.Id,
                    schedule.Status,
                    HoursWorked = hoursWorked
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating WorkSchedule status");
                return StatusCode(500, ApiResponseDto.ErrorResult("Failed to update WorkSchedule status"));
            }
        }
    }
}

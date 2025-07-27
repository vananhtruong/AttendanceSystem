using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Services;
using BusinessObject.DTOs;

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

        public FaceAttendanceController(
            IFaceRecognitionService faceRecognitionService,
            ILogger<FaceAttendanceController> logger)
        {
            _faceRecognitionService = faceRecognitionService;
            _logger = logger;
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
    }
}

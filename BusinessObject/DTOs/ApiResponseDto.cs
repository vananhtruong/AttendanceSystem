namespace BusinessObject.DTOs
{
    public class ApiResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public object Errors { get; set; }

        public static ApiResponseDto SuccessResult(object data = null, string message = "Success")
        {
            return new ApiResponseDto
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponseDto ErrorResult(string message, object errors = null)
        {
            return new ApiResponseDto
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
} 
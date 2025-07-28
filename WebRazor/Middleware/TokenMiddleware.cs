using Microsoft.AspNetCore.Http;

namespace WebRazor.Middleware
{
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Lấy token từ session
            var token = context.Session.GetString("AccessToken");
            
            if (!string.IsNullOrEmpty(token))
            {
                // Thêm token vào header Authorization nếu chưa có
                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Request.Headers.Add("Authorization", $"Bearer {token}");
                }
            }

            await _next(context);
        }
    }
} 
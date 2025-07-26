using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using BusinessObject.Models;
using AutoMapper;
using BusinessObject.DTOs;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public NotificationController(INotificationRepository notificationRepo, IUserRepository userRepo, IMapper mapper)
        {
            _notificationRepo = notificationRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        // GET: api/Notification/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var notifications = await _notificationRepo.GetByUserIdAsync(userId);
            var dto = _mapper.Map<List<NotificationDTO>>(notifications);
            return Ok(dto);
        }

        // POST: api/Notification/send
        [HttpPost("send")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDTO dto)
        {
            var notification = _mapper.Map<Notification>(dto);
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;
            await _notificationRepo.AddAsync(notification);
            // TODO: Gửi mail cho user nếu cần
            return Ok();
        }

        // PUT: api/Notification/read/{id}
        [HttpPut("read/{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _notificationRepo.GetByIdAsync(id);
            if (notification == null) return NotFound();
            notification.IsRead = true;
            await _notificationRepo.UpdateAsync(notification);
            return Ok();
        }

        // GET: api/Notification/count
        [HttpGet("count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetNotificationCount()
        {
            var notifications = await _notificationRepo.GetAllAsync();
            var count = notifications.Count();
            return Ok(count);
        }
    }
} 
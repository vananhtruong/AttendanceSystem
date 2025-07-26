using BusinessObject.Models;
using DataAccessLayer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDAO _notificationDAO;

        public NotificationRepository(NotificationDAO notificationDAO)
        {
            _notificationDAO = notificationDAO;
        }

        public Task<Notification?> GetByIdAsync(int id)
            => _notificationDAO.GetByIdAsync(id);
        public Task<List<Notification>> GetByUserIdAsync(int userId)
            => _notificationDAO.GetByUserIdAsync(userId);
        public Task<List<Notification>> GetAllAsync()
            => _notificationDAO.GetAllAsync();
        public Task AddAsync(Notification notification)
            => _notificationDAO.AddAsync(notification);
        public Task UpdateAsync(Notification notification)
            => _notificationDAO.UpdateAsync(notification);
        public Task DeleteAsync(int id)
            => _notificationDAO.DeleteAsync(id);
    }
}

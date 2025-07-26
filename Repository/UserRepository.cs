using BusinessObject.Models;
using DataAccessLayer;

namespace Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDAO _userDao;

        public UserRepository(UserDAO userDao)
        {
            _userDao = userDao;
        }

        public Task<User?> GetByEmailAsync(string email)
            => _userDao.GetByEmailAsync(email);

        public Task<User?> GetByIdAsync(int id)
            => _userDao.GetByIdAsync(id);

        public Task<List<User>> GetAllAsync()
            => _userDao.GetAllAsync();

        public Task<List<User>> SearchAsync(string keyword)
            => _userDao.SearchAsync(keyword);

        public Task AddAsync(User user)
            => _userDao.AddAsync(user);

        public Task UpdateAsync()
            => _userDao.UpdateAsync();

        public Task UpdateUserAsync(User user)
            => _userDao.UpdateUserAsync(user);

        public Task DeleteAsync(int id)
            => _userDao.DeleteAsync(id);

        public Task SetRoleAsync(int userId, string role)
            => _userDao.SetRoleAsync(userId, role);
    }
}

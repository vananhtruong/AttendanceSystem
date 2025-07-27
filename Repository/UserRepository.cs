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

        public Task AddAsync(User user)
            => _userDao.AddAsync(user);

        public Task<List<User>> GetAllAsync()
            => _userDao.GetAllAsync();

        public Task UpdateAsync(User user)
            => _userDao.UpdateAsync(user);
    }

}

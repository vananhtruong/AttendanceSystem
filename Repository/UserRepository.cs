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

        public Task AddAsync(User user)
            => _userDao.AddAsync(user);

        public Task<List<User>> GetAllAsync()
            => _userDao.GetAllAsync();

        public Task UpdateAsync()
            => _userDao.UpdateAsync();
    }

}

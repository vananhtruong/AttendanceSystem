using BusinessObject.Models;

namespace Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(); 
        Task<List<User>> GetAllAsync();
    }

}

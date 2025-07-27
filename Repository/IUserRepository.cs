﻿using BusinessObject.Models;

namespace Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
        Task UpdateAsync(User user); 
        Task<List<User>> GetAllAsync();
    }

}

using BusinessObject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface ICorrectionRequestRepository
    {
        Task<CorrectionRequest?> GetByIdAsync(int id);
        Task<List<CorrectionRequest>> GetByUserIdAsync(int userId);
        Task<List<CorrectionRequest>> GetAllAsync();
        Task AddAsync(CorrectionRequest request);
        Task UpdateAsync(CorrectionRequest request);
        Task DeleteAsync(int id);
    }
} 
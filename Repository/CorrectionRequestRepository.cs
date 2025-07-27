using BusinessObject.Models;
using DataAccessLayer;

namespace Repository
{
    public class CorrectionRequestRepository : ICorrectionRequestRepository
    {
        private readonly CorrectionRequestDAO _correctionRequestDao;

        public CorrectionRequestRepository(CorrectionRequestDAO correctionRequestDao)
        {
            _correctionRequestDao = correctionRequestDao;
        }

        public Task<CorrectionRequest?> GetByIdAsync(int id)
            => _correctionRequestDao.GetByIdAsync(id);

        public Task<List<CorrectionRequest>> GetByUserIdAsync(int userId)
            => _correctionRequestDao.GetByUserIdAsync(userId);

        public Task<List<CorrectionRequest>> GetAllAsync()
            => _correctionRequestDao.GetAllAsync();

        public Task AddAsync(CorrectionRequest request)
            => _correctionRequestDao.AddAsync(request);

        public Task UpdateAsync(CorrectionRequest request)
            => _correctionRequestDao.UpdateAsync(request);

        public Task DeleteAsync(int id)
            => _correctionRequestDao.DeleteAsync(id);
    }
} 
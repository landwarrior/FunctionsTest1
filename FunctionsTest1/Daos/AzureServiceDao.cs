using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FunctionsTest1.DAL.Contexts;
using FunctionsTest1.DAL.Models;

namespace FunctionsTest1.Daos
{
    public class AzureServiceDao : IAzureServiceDao
    {
        private readonly TestDbContext _dbContext;
        public AzureServiceDao(TestDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<AzureService>> GetAllAsync()
        {
            return _dbContext.AzureServices.ToList();
        }
        public async Task<AzureService> GetByIdAsync(string id)
        {
            return _dbContext.AzureServices.FirstOrDefault(e => e.Id == id);
        }
        public async Task AddOrUpdateAsync(AzureService entity)
        {
            var existing = _dbContext.AzureServices.FirstOrDefault(e => e.Id == entity.Id);
            if (existing == null)
            {
                _dbContext.AzureServices.Add(entity);
            }
            else
            {
                existing.Name = entity.Name;
                existing.Type = entity.Type;
                existing.DisplayName = entity.DisplayName;
                existing.ResourceType = entity.ResourceType;
            }
        }
        public async Task RemoveByIdsAsync(IEnumerable<string> ids)
        {
            var removeEntities = _dbContext.AzureServices.Where(e => ids.Contains(e.Id)).ToList();
            _dbContext.AzureServices.RemoveRange(removeEntities);
        }
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

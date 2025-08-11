using FunctionsTest1.DAL.Contexts;
using FunctionsTest1.DAL.Models;
using Microsoft.EntityFrameworkCore;

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
            return await _dbContext.AzureServices.ToListAsync();
        }
        public async Task<AzureService?> GetByIdAsync(string id)
        {
            return await _dbContext.AzureServices.FirstOrDefaultAsync(e => e.Id == id);
        }
        public async Task AddOrUpdateAsync(AzureService entity)
        {
            var existing = await _dbContext.AzureServices.FirstOrDefaultAsync(e => e.Id == entity.Id);
            var now = DateTime.UtcNow;
            if (existing == null)
            {
                entity.CreatedAt = now;
                entity.CreateUser = "Functions";
                entity.UpdatedAt = now;
                entity.UpdateUser = "Functions";
                await _dbContext.AzureServices.AddAsync(entity);
            }
            else
            {
                existing.Name = entity.Name;
                existing.Type = entity.Type;
                existing.DisplayName = entity.DisplayName;
                existing.ResourceType = entity.ResourceType;
                existing.UpdatedAt = now;
                existing.UpdateUser = "Functions";
            }
        }
        public async Task RemoveByIdsAsync(IEnumerable<string> ids)
        {
            var removeEntities = await _dbContext.AzureServices.Where(e => ids.Contains(e.Id)).ToListAsync();
            _dbContext.AzureServices.RemoveRange(removeEntities);
        }
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

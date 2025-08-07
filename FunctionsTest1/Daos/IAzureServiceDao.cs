using System.Collections.Generic;
using System.Threading.Tasks;
using FunctionsTest1.DAL.Models;

namespace FunctionsTest1.Daos
{
    public interface IAzureServiceDao
    {
        Task<List<AzureService>> GetAllAsync();
        Task<AzureService> GetByIdAsync(string id);
        Task AddOrUpdateAsync(AzureService entity);
        Task RemoveByIdsAsync(IEnumerable<string> ids);
        Task SaveChangesAsync();
    }
}

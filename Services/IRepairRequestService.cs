using MAMONT.Models;

namespace MAMONT.Services;

public interface IRepairRequestService
{
    Task CreateTableAsync();
    Task AddAsync(RepairRequestForm request);
    Task<List<RepairRequestItem>> GetAllAsync();
    Task UpdateStatusAsync(int id, string status);
}

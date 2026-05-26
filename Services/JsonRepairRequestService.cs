using System.Text.Json;
using MAMONT.Models;

namespace MAMONT.Services;

public class JsonRepairRequestService : IRepairRequestService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public JsonRepairRequestService(IWebHostEnvironment environment)
    {
        var folderPath = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(folderPath);
        _filePath = Path.Combine(folderPath, "repair_requests.json");
    }

    public async Task CreateTableAsync()
    {
        if (!File.Exists(_filePath))
        {
            await File.WriteAllTextAsync(_filePath, "[]");
        }
    }

    public async Task AddAsync(RepairRequestForm request)
    {
        await _lock.WaitAsync();

        try
        {
            var requests = await ReadAllInternalAsync();
            var nextId = requests.Count == 0 ? 1 : requests.Max(item => item.Id) + 1;

            requests.Add(new RepairRequestItem
            {
                Id = nextId,
                Name = request.Name,
                Phone = request.Phone,
                Messenger = request.Messenger,
                RepairType = request.RepairType,
                Area = request.Area,
                Comment = request.Comment,
                EstimatedPrice = request.EstimatedPrice,
                CreatedAt = DateTime.UtcNow
            });

            await WriteAllInternalAsync(requests);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<RepairRequestItem>> GetAllAsync()
    {
        await _lock.WaitAsync();

        try
        {
            var requests = await ReadAllInternalAsync();
            return requests.OrderByDescending(item => item.CreatedAt).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<RepairRequestItem>> ReadAllInternalAsync()
    {
        await CreateTableAsync();
        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<RepairRequestItem>>(json) ?? [];
    }

    private async Task WriteAllInternalAsync(List<RepairRequestItem> requests)
    {
        var json = JsonSerializer.Serialize(requests, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}

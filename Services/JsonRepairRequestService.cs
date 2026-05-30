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
                CreatedAt = DateTime.UtcNow,
                Status = RepairRequestStatus.New
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

    public async Task UpdateStatusAsync(int id, string status)
    {
        if (!RepairRequestStatus.IsValid(status))
        {
            throw new ArgumentException("Недопустимый статус заявки.", nameof(status));
        }

        await _lock.WaitAsync();

        try
        {
            var requests = await ReadAllInternalAsync();
            var request = requests.FirstOrDefault(item => item.Id == id);

            if (request is not null)
            {
                request.Status = status;
                await WriteAllInternalAsync(requests);
            }
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
        var requests = JsonSerializer.Deserialize<List<RepairRequestItem>>(json) ?? [];

        foreach (var request in requests.Where(item => string.IsNullOrWhiteSpace(item.Status)))
        {
            request.Status = RepairRequestStatus.New;
        }

        return requests;
    }

    private async Task WriteAllInternalAsync(List<RepairRequestItem> requests)
    {
        var json = JsonSerializer.Serialize(requests, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}

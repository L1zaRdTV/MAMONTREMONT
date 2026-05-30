using MAMONT.Models;
using Npgsql;

namespace MAMONT.Services;

public class RepairRequestService : IRepairRequestService
{
    private readonly NpgsqlDataSource _dataSource;

    public RepairRequestService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task CreateTableAsync()
    {
        const string sql = """
            create table if not exists repair_requests (
                id serial primary key,
                name varchar(100) not null,
                phone varchar(50) not null,
                messenger varchar(100),
                repair_type varchar(50) not null,
                area integer not null,
                comment text,
                estimated_price numeric(12, 2) not null,
                created_at timestamptz not null default now(),
                status varchar(20) not null default 'new'
            );

            alter table repair_requests
            add column if not exists status varchar(20) not null default 'new';

            update repair_requests
            set status = 'processed'
            where status in ('accepted', 'cancelled');
            """;

        await using var command = _dataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync();
    }

    public async Task AddAsync(RepairRequestForm request)
    {
        const string sql = """
            insert into repair_requests
                (name, phone, messenger, repair_type, area, comment, estimated_price, status)
            values
                (@name, @phone, @messenger, @repair_type, @area, @comment, @estimated_price, @status);
            """;

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("name", request.Name);
        command.Parameters.AddWithValue("phone", request.Phone);
        command.Parameters.AddWithValue("messenger", (object?)request.Messenger ?? DBNull.Value);
        command.Parameters.AddWithValue("repair_type", request.RepairType);
        command.Parameters.AddWithValue("area", request.Area);
        command.Parameters.AddWithValue("comment", (object?)request.Comment ?? DBNull.Value);
        command.Parameters.AddWithValue("estimated_price", request.EstimatedPrice);
        command.Parameters.AddWithValue("status", RepairRequestStatus.New);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<RepairRequestItem>> GetAllAsync()
    {
        const string sql = """
            select id, name, phone, messenger, repair_type, area, comment, estimated_price, created_at, status
            from repair_requests
            order by created_at desc;
            """;

        var requests = new List<RepairRequestItem>();
        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            requests.Add(new RepairRequestItem
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Phone = reader.GetString(2),
                Messenger = reader.IsDBNull(3) ? null : reader.GetString(3),
                RepairType = reader.GetString(4),
                Area = reader.GetInt32(5),
                Comment = reader.IsDBNull(6) ? null : reader.GetString(6),
                EstimatedPrice = reader.GetDecimal(7),
                CreatedAt = reader.GetDateTime(8),
                Status = RepairRequestStatus.Normalize(reader.IsDBNull(9) ? null : reader.GetString(9))
            });
        }

        return requests;
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        if (!RepairRequestStatus.IsValid(status))
        {
            throw new ArgumentException("Недопустимый статус заявки.", nameof(status));
        }

        const string sql = """
            update repair_requests
            set status = @status
            where id = @id;
            """;

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("status", status);

        await command.ExecuteNonQueryAsync();
    }
}

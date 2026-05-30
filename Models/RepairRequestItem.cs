namespace MAMONT.Models;

public class RepairRequestItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Messenger { get; set; }
    public string RepairType { get; set; } = "";
    public int Area { get; set; }
    public string? Comment { get; set; }
    public decimal EstimatedPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = RepairRequestStatus.New;
}

public static class RepairRequestStatus
{
    public const string New = "new";
    public const string Accepted = "accepted";
    public const string Cancelled = "cancelled";

    public static bool IsValid(string? status) => status is New or Accepted or Cancelled;

    public static string GetLabel(string? status) => status switch
    {
        Accepted => "Принята",
        Cancelled => "Отменена",
        _ => "Новая"
    };
}

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
}

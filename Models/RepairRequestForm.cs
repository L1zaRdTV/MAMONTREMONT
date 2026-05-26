using System.ComponentModel.DataAnnotations;

namespace MAMONT.Models;

public class RepairRequestForm
{
    [Required(ErrorMessage = "Введите имя")]
    [StringLength(100, ErrorMessage = "Имя не должно быть длиннее 100 символов")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Введите телефон")]
    [StringLength(50, ErrorMessage = "Телефон не должен быть длиннее 50 символов")]
    public string Phone { get; set; } = "";

    [StringLength(100, ErrorMessage = "Мессенджер не должен быть длиннее 100 символов")]
    public string? Messenger { get; set; }

    [Required(ErrorMessage = "Выберите тип ремонта")]
    [StringLength(50, ErrorMessage = "Тип ремонта не должен быть длиннее 50 символов")]
    public string RepairType { get; set; } = "Косметический";

    [Range(10, 300, ErrorMessage = "Площадь должна быть от 10 до 300 м²")]
    public int Area { get; set; } = 40;

    [StringLength(3000, ErrorMessage = "Комментарий не должен быть длиннее 3000 символов")]
    public string? Comment { get; set; }

    public decimal EstimatedPrice { get; set; }
}

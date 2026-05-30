using MAMONT.Models;
using MAMONT.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MAMONT.Pages;

public class RequestsModel : PageModel
{
    private const string AdminPassword = "123";
    private const string SessionKey = "RequestsUnlocked";
    private readonly IRepairRequestService _requestService;

    public RequestsModel(IRepairRequestService requestService)
    {
        _requestService = requestService;
    }

    public List<RepairRequestItem> Requests { get; set; } = [];
    public bool IsUnlocked { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StatusMessage { get; set; }

    [BindProperty]
    [StringLength(10, ErrorMessage = "Пароль не должен быть длиннее 10 символов")]
    public string Password { get; set; } = "";

    public async Task OnGetAsync()
    {
        await LoadRequestsIfUnlockedAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Пароль не должен быть длиннее 10 символов";
            await LoadRequestsIfUnlockedAsync();
            return Page();
        }

        if (Password == AdminPassword)
        {
            HttpContext.Session.SetString(SessionKey, "true");
            return RedirectToPage();
        }

        ErrorMessage = "Неверный пароль";
        await LoadRequestsIfUnlockedAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
    {
        if (HttpContext.Session.GetString(SessionKey) != "true")
        {
            ErrorMessage = "Сначала войдите в админку.";
            await LoadRequestsIfUnlockedAsync();
            return Page();
        }

        if (id <= 0 || !RepairRequestStatus.IsValid(status))
        {
            ErrorMessage = "Не удалось обновить статус заявки.";
            await LoadRequestsIfUnlockedAsync();
            return Page();
        }

        await _requestService.UpdateStatusAsync(id, status);

        TempData["StatusMessage"] = status == RepairRequestStatus.Processed
            ? $"Заявка #{id} перенесена в архив как отработанная."
            : $"Заявка #{id} возвращена в новые.";

        return RedirectToPage();
    }

    private async Task LoadRequestsIfUnlockedAsync()
    {
        IsUnlocked = HttpContext.Session.GetString(SessionKey) == "true";

        if (IsUnlocked)
        {
            StatusMessage = TempData["StatusMessage"] as string;
            Requests = await _requestService.GetAllAsync();
        }
    }
}

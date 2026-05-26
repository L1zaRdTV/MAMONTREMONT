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

    private async Task LoadRequestsIfUnlockedAsync()
    {
        IsUnlocked = HttpContext.Session.GetString(SessionKey) == "true";

        if (IsUnlocked)
        {
            Requests = await _requestService.GetAllAsync();
        }
    }
}

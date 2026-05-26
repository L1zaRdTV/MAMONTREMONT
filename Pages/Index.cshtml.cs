using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MAMONT.Models;
using MAMONT.Services;

namespace MAMONT.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly RepairRequestService _requestService;

        public IndexModel(ILogger<IndexModel> logger, RepairRequestService requestService)
        {
            _logger = logger;
            _requestService = requestService;
        }

        [BindProperty]
        public RepairRequestForm RequestForm { get; set; } = new();

        public string? SuccessMessage { get; set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            RequestForm.EstimatedPrice = CalculatePrice(RequestForm.RepairType, RequestForm.Area);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _requestService.AddAsync(RequestForm);
            SuccessMessage = "Заявка отправлена. Мы свяжемся с вами в ближайшее время.";
            ModelState.Clear();
            RequestForm = new RepairRequestForm();

            return Page();
        }

        public static decimal CalculatePrice(string repairType, int area)
        {
            var pricePerMeter = repairType switch
            {
                "Капитальный" => 12000,
                "Под ключ" => 18000,
                _ => 6500
            };

            return area * pricePerMeter;
        }
    }
}

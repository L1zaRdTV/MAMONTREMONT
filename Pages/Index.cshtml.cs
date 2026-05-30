using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MAMONT.Models;
using MAMONT.Services;

namespace MAMONT.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IRepairRequestService _requestService;

        public IndexModel(ILogger<IndexModel> logger, IRepairRequestService requestService)
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
            SuccessMessage = "Мы получили ваши контакты и скоро свяжемся для уточнения деталей ремонта.";
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

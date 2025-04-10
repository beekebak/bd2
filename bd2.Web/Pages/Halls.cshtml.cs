using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using bd2.Application.Services;
using bd2.Core.PerformanceAggregate;

namespace bd2.Web.Pages
{
    public class HallsModel : PageModel
    {
        private readonly PerformanceManagementService _performanceService;

        public HallsModel(PerformanceManagementService performanceService)
        {
            _performanceService = performanceService;
        }

        public List<Hall> HallsList { get; set; }

        public void OnGet()
        {
            HallsList = _performanceService.GetHalls().ToList();
        }

        public IActionResult OnPostDelete(int hallId)
        {
            _performanceService.DeleteHall(hallId);
            return RedirectToPage();
        }

        public IActionResult OnPostUpdate(int hallId, int capacity)
        {
            _performanceService.UpdateHall(new Hall(hallId, capacity));
            return RedirectToPage();
        }

        public IActionResult OnPostAdd(int capacity)
        {
            _performanceService.AddHall(new Hall(0, capacity));
            return RedirectToPage();
        }
    }
}
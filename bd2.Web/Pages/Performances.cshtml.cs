using bd2.Application.Services;
using bd2.Core;
using bd2.Core.PerformanceAggregate;
using bd2.Core.StagingAggregate;
using bd2.Core.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class PerformancesModel : PageModel
{
    private readonly PerformanceManagementService _performanceService;

    public PerformancesModel(PerformanceManagementService performanceService)
    {
        _performanceService = performanceService;
    }

    public List<Performance> PerformanceList { get; set; }
    public List<Staging> StagingList { get; set; }
    public List<Hall> HallList { get; set; }
    public List<Artist> ArtistList { get; set; }

    public void OnGet()
    {
        PerformanceList = _performanceService.GetPerformances().ToList();
        StagingList = _performanceService.GetStagings();
        HallList = _performanceService.GetHalls().ToList();
        ArtistList = _performanceService.GetArtists();
    }

    public IActionResult OnPostDelete(int performanceId)
    {
        _performanceService.DeletePerformance(performanceId);
        return RedirectToPage();
    }

    public IActionResult OnPostAdd(DateTime date, TimeOnly startTime, int stagingId, int hallId, Dictionary<int, int>? actorIds)
    {
        if (actorIds != null && actorIds.Values.Distinct().Count() != actorIds.Values.Count)
        {
            ModelState.AddModelError(string.Empty, "Один актер не может играть несколько ролей.");
            return Page();
        }
        
        _performanceService.AddPerformance(new Performance(
            id:0,
            startDate:new DateTime(DateOnly.FromDateTime(date), startTime),
            staging:new Staging(stagingId, startTime, new Worker(0, "", ""), 
                new Worker(0, "", ""),
                new Origin(0, "", new Author(0, "", []), new Author(0 ,"", [])),
                [], new Dictionary<Inventory, int>()),
            hall:new Hall(hallId, 0),
            soldTicketsCount:0,
            new List<Performance.ArtistsInPerformance>(actorIds.Select(p => new Performance.ArtistsInPerformance(p.Value, p.Key)))));
        return RedirectToPage();
    }
    
    public IActionResult OnGetGetRoles(int stagingId)
    {
        var staging = _performanceService.GetStagingById(stagingId);
        return new JsonResult(staging.Roles);
    }
}
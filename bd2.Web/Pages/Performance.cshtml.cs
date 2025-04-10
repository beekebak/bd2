using bd2.Application.Services;
using bd2.Core.PerformanceAggregate;
using bd2.Core.StagingAggregate;
using bd2.Core.Worker;
using bd2.Infrastructure.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class PerformanceModel : PageModel
{
    private readonly PerformanceManagementService _performanceService;

    public PerformanceModel(PerformanceManagementService performanceService)
    {
        _performanceService = performanceService;
        
    }

    public Performance Performance { get; set; }
    public List<Hall> Halls { get; set; }
    public List<Artist> Artists { get; set; }
    public Dictionary<Artist, Role> Roles { get; set; }

    public IActionResult OnGet(int id)
    {
        Performance = _performanceService.GetPerformance(id);
        Halls = _performanceService.GetHalls().ToList();
        Artists = _performanceService.GetArtists().ToList();
        var artist = _performanceService.GetArtists(Performance.Artists.Select(x => x.ArtistId).ToArray());
        var roles = _performanceService.GetRoles(Performance.Artists.Select(x => x.RoleId).ToArray());
        Roles = artist.Select(a => new KeyValuePair<Artist, Role>(a,
            roles.Find(r => r.Id == Performance.Artists.Find(ar => ar.ArtistId == a.Id)!.RoleId)!)).ToDictionary();
        return Page();
    }

    public IActionResult OnPostUpdateArtistRole(int performanceId, int roleId, int artistId)
    {
        _performanceService.UpdateArtistRole(performanceId, roleId, artistId);
        return RedirectToPage(new { id = performanceId });
    }

    public IActionResult OnPostUpdateHall(int performanceId, int hallId)
    {
        _performanceService.MoveToOtherHall(performanceId, hallId);
        return RedirectToPage(new { id = performanceId });
    }

    public IActionResult OnPostBuyTicket(int performanceId)
    {
        _performanceService.BuyTicket(performanceId);
        return RedirectToPage(new { id = performanceId });
    }

    public IActionResult OnPostReturnTicket(int performanceId)
    {
        _performanceService.ReturnTicket(performanceId);
        return RedirectToPage(new { id = performanceId });
    }
}
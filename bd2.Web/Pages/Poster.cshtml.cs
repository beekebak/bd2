using bd2.Application.DTO;
using bd2.Application.Services;
using bd2.Core.PerformanceAggregate;
using bd2.Core.Worker;
using bd2.Infrastructure.DTO;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class PosterModel : PageModel
{
    private readonly PerformanceManagementService _performanceService;

    public PosterModel(PerformanceManagementService performanceService)
    {
        _performanceService = performanceService;
    }

    public List<FilteredPerformance> Performances { get; set; }
    public Dictionary<int, Artist> Artists { get; set; }
    public PerformanceFilter Filter { get; set; }

    public void OnGet(PerformanceFilter filter)
    {
        Filter = filter;
        Performances = _performanceService.GetPerformances(filter).ToList();
        var artists = _performanceService.GetArtists();
        Artists = artists.ToDictionary(a => a.Id, a => a);
    }
}
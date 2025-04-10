using bd2.Application.DTO;
using bd2.Application.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class RepertoireModel : PageModel
{
    private readonly StagingManagementService _stagingService;

    public RepertoireModel(StagingManagementService stagingService)
    {
        _stagingService = stagingService;
    }

    public List<FilteredStaging> Stagings { get; set; }
    public StagingFilter Filter { get; set; }

    public void OnGet(StagingFilter filter)
    {
        Filter = filter;
        Stagings = _stagingService.GetStagings(filter).ToList();
    }
}
using bd2.Application.Services;
using bd2.Core;
using bd2.Core.StagingAggregate;
using bd2.Core.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class StagingsModel : PageModel
{
    private readonly StagingManagementService _stagingService;

    public StagingsModel(StagingManagementService stagingService)
    {
        _stagingService = stagingService;
    }

    public List<Staging> StagingList { get; set; }
    public List<Origin> OriginList { get; set; }
    public List<Worker> WorkerList { get; set; }
    public List<Inventory> InventoryList { get; set; }

    public void OnGet()
    {
        StagingList = _stagingService.GetStagings().ToList();
        OriginList = _stagingService.GetAllOrigins();
        WorkerList = _stagingService.GetAllWorkers();
        InventoryList = _stagingService.GetAllInventory();
    }

    public IActionResult OnPostDelete(int stagingId)
    {
        _stagingService.DeleteStaging(stagingId);
        return RedirectToPage();
    }

    public IActionResult OnPostAdd(TimeOnly duration, int directorId, int stagingComposerId, int originId, string[] roles, int[] inventoryKeys, int[] inventoryValues)
    {
        var inventory = new Dictionary<Inventory, int>();
        for (int i = 0; i < inventoryKeys.Length; i++)
        {
            inventory.Add(new Inventory("", inventoryKeys[i], 0), inventoryValues[i]);
        }

        _stagingService.AddStaging(new Staging(0, duration, new Worker(directorId, "", ""), 
            new Worker(stagingComposerId, "", ""), new Origin(originId, "", 
                new Author(0, "", []), new Author(0, "", [])),
            roles.Select(r => new Role(0, r)).ToList(), inventory));
        return RedirectToPage();
    }
}
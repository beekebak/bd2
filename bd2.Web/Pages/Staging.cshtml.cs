using bd2.Application.Services;
using bd2.Core;
using bd2.Core.StagingAggregate;
using bd2.Core.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class StagingModel : PageModel
{
    private readonly StagingManagementService _stagingService;

    public StagingModel(StagingManagementService stagingService)
    {
        _stagingService = stagingService;
    }

    public Staging Staging { get; set; }
    
    public List<Origin> OriginList { get; set; }
    public List<Worker> WorkerList { get; set; }
    public List<Inventory> InventoryList { get; set; }

    public IActionResult OnGet(int id)
    {
        Staging = _stagingService.GetStaging(id);
        OriginList = _stagingService.GetAllOrigins();
        WorkerList = _stagingService.GetAllWorkers();
        InventoryList = _stagingService.GetAllInventory();
        return Page();
    }

    public IActionResult OnPostUpdate(int stagingId, TimeOnly duration, int directorId, int stagingComposerId, int originId, string[] roles, int[] inventoryKeys, int[] inventoryValues)
    {
        var inventory = new Dictionary<Inventory, int>();
        for (int i = 0; i < inventoryKeys.Length; i++)
        {
            if (inventory.Keys.Any(k => k.InventoryId == inventoryKeys[i]))
            {
                inventory[inventory.Keys.First(k => k.InventoryId == inventoryKeys[i])] += inventoryValues[i];
            }
            else inventory.Add(new Inventory("", inventoryKeys[i], 0), inventoryValues[i]);
        }

        _stagingService.UpdateStaging(new Staging(stagingId, duration, new Worker(directorId, "", ""), 
            new Worker(stagingComposerId, "", ""), new Origin(originId, "", 
                new Author(0, "", []), new Author(0, "", [])),
            roles.Select(r => new Role(0, r)).ToList(), inventory));
        return RedirectToPage(new { id = stagingId });
    }
}
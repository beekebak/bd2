using bd2.Application.Services;
using bd2.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class InventoryModel : PageModel
{
    private readonly InventoryManagementService _inventoryService;

    public InventoryModel(InventoryManagementService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public List<Inventory> InventoryItems { get; set; }

    public void OnGet()
    {
        InventoryItems = _inventoryService.GetInventory().ToList();
    }

    public IActionResult OnPostDelete(int itemId)
    {
        _inventoryService.DeleteInventory(itemId);
        return RedirectToPage();
    }

    public IActionResult OnPostUpdate(int itemId, string inventoryName, int totalAmount)
    {
        _inventoryService.UpdateInventory(new Inventory(inventoryName, itemId, totalAmount));
        return RedirectToPage();
    }
    
    public IActionResult OnPostAdd(string inventoryName, int totalAmount)
    {
        _inventoryService.AddInventory(new Inventory(inventoryName, 0, totalAmount));
        return RedirectToPage();
    }
}
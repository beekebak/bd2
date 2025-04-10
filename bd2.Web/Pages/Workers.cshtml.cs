using bd2.Application.Services;
using bd2.Core.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class WorkersModel : PageModel
{
    private readonly WorkerManagementService _workersService;

    public WorkersModel(WorkerManagementService workersService)
    {
        _workersService = workersService;
    }

    public List<Worker> WorkersList { get; set; }

    public void OnGet()
    {
        WorkersList = _workersService.GetWorkers().ToList();
    }

    public IActionResult OnPostDelete(int workerId)
    {
        _workersService.DeleteWorker(workerId);
        return RedirectToPage();
    }

    public IActionResult OnPostUpdateWorker(int workerId, string name, string specialty)
    {
        _workersService.UpdateWorker(new Worker(workerId, name, specialty));
        return RedirectToPage();
    }
    
    public IActionResult OnPostUpdateArtist(int workerId, string name, string specialty, string grade)
    {
        _workersService.UpdateWorker(new Artist(workerId, name, specialty, grade));
        return RedirectToPage();
    }

    public IActionResult OnPostAddWorker(string name, string specialty)
    {
        _workersService.AddWorker(new Worker(0, name, specialty));
        return RedirectToPage();
    }

    public IActionResult OnPostAddArtist(string name, string grade)
    {
        _workersService.AddWorker(new Artist(0, name, "Актер", grade));
        return RedirectToPage();
    }
}
using bd2.Application.Services;
using bd2.Core.StagingAggregate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class OriginsModel : PageModel
{
    private readonly OriginsManagementService _originService;

    public OriginsModel(OriginsManagementService originService)
    {
        _originService = originService;
    }

    public List<Origin> OriginsList { get; set; }
    public List<Author> ComposersList { get; set; }
    public List<Author> WritersList { get; set; }

    public void OnGet()
    {
        OriginsList = _originService.GetOrigins().ToList();
        var authors = _originService.GetAuthors().ToList();
        ComposersList = authors.Where(x => x.Specialties.Any(s => s.SpecialtyName == "Композитор")).ToList();
        WritersList = authors.Where(x => x.Specialties.Any(s => s.SpecialtyName == "Писатель")).ToList();
    }

    public IActionResult OnPostDelete(int originId)
    {
        _originService.DeleteOrigin(originId);
        return RedirectToPage();
    }

    public IActionResult OnPostAdd(int originId, string originName, int originComposerId, int writerId)
    {
        _originService.AddOrigin(new Origin(originId, originName, new Author(writerId, "", []), new Author(originComposerId, "", [])));
        return RedirectToPage();
    }
    
    public IActionResult OnPostUpdate(int originId, string originName, int originComposerId, int writerId)
    {
        _originService.UpdateOrigin(new Origin(originId, originName, new Author(writerId, "", []), new Author(originComposerId, "", [])));
        return RedirectToPage();
    }
}
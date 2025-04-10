using bd2.Application.Services;
using bd2.Core.StagingAggregate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class AuthorsModel : PageModel
{
    private readonly OriginsManagementService _authorService;

    public AuthorsModel(OriginsManagementService authorService)
    {
        _authorService = authorService;
    }

    public List<Author> AuthorsList { get; set; } = new List<Author>();
    public void OnGet()
    {
        AuthorsList = _authorService.GetAuthors().ToList();
    }

    public IActionResult OnPostAdd(string authorName, string[] specialties)
    {
        if (ModelState.IsValid)
        {
            _authorService.AddAuthor(new Author(1, authorName, 
                new List<AuthorSpecialty>(specialties.Select(x => new AuthorSpecialty(x)))));
            return RedirectToPage();
        }

        return Page();
    }

    public IActionResult OnPostDelete(int authorId)
    {
        _authorService.DeleteAuthor(authorId);
        return new OkResult();
    }
    
    public IActionResult OnPostUpdate(int id, string authorName, string[] specialties)
    {
        _authorService.UpdateAuthor(new Author(id, authorName, specialties.Select(s => new AuthorSpecialty(s)).ToList()));
        return RedirectToPage();
    }
}
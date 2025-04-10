using bd2.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        AuthHelper.ClearAuthCookie(HttpContext);
        return RedirectToPage("/Authorization");
    }
}
using bd2.Application.Services;
using bd2.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class AuthorizationModel(UserManagementService service) : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }

    public RedirectResult OnPost(string login, string password)
    {
        var user = service.GetUserPlainPassword(login, password);
        if (user != null)
        {
            AuthHelper.SetAuthCookie(HttpContext, user.Login, user.HashedPassword, user.Role);
        }
        return Redirect("/");
    }

    public RedirectResult OnPostGuest()
    {
        AuthHelper.SetAuthCookie(HttpContext, "", "", "guest");
        return Redirect("/");
    }
}
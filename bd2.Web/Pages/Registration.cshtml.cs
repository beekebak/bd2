using bd2.Application.Services;
using bd2.Infrastructure.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bd2.Web.Pages;

public class RegistrationModel(UserManagementService userService) : PageModel
{
    [BindProperty]
    public int CurrentUserId { get; set; }
    public UserDto? User { get; set; }
    
    public void OnGet()
    {
        if (HttpContext.Items["User"] is UserDto user)
        {
            CurrentUserId = user.Id;
            User = user;
        }
    }
    
    public IActionResult OnPostAddUserAsync(string login, string password, string role)
    {
        userService.RegisterUser(login, password, role);
        return Page();
    }

    public IActionResult OnPostChangePasswordAsync(string oldPassword, string newPassword, string login, string? role)
    {
        role ??= User!.Role;
        var oldLogin = User!.Login;
        var result = userService.UpdateUser(User.Id, oldLogin, login, oldPassword, newPassword, role);
        if (result)
        {
            TempData["SuccessMessage"] = "Пароль успешно изменен.";
            return RedirectToPage();
        }
        ModelState.AddModelError(string.Empty, "Ошибка при изменении пароля.");
        return Page();
    }
}
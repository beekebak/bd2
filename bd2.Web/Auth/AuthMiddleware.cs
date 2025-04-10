using System.Data;
using System.Security.Claims;
using bd2.Application.Services;
using bd2.Infrastructure.DTO;

namespace bd2.Web.Auth;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private UserManagementService _service;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        _service = context.RequestServices.GetRequiredService<UserManagementService>();
        var path = context.Request.Path.Value?.ToLower();

        if (path != null && (path.Contains("css") || path.Contains("js") || path.Contains("images") ||
                             path.Contains(".ico")))
        {
            return _next(context);
        }

        var publicPages = new[] { "/authorization" };

        if (publicPages.Contains(path))
        { 
            return _next(context);
        }

        var auth = AuthHelper.GetAuthCookie(context);

        if (auth != null)
        {
            var (login, hash, role) = auth.Value;

            if (role == "guest")
            {
                context.Items["User"] = new UserDto
                {
                    Login = login,
                    HashedPassword = hash,
                    Role = "guest"
                };
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "guest")
                };

                var identity = new ClaimsIdentity(claims, "CustomAuth");
                context.User = new ClaimsPrincipal(identity);
                RelogDb(context, "guest");
                try
                {
                    return _next(context);
                }
                catch (Exception ex)
                {
                    context.Response.Redirect("/error");
                }
                finally
                {
                    ResetRole(context);
                }
            }

            var user = _service.GetUserHashedPassword(login, hash);
            if (user != null)
            {
                context.Items["User"] = user;
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, user.Role)
                };
                var identity = new ClaimsIdentity(claims, "CustomAuth");
                context.User = new ClaimsPrincipal(identity);
                RelogDb(context, user.Role);
                try
                { 
                    return _next(context);
                }
                catch (Exception ex)
                {
                    context.Response.Redirect("/error");
                }
                finally
                {
                    ResetRole(context);
                }
            }
        }

        context.Response.Redirect("/authorization");
        return Task.CompletedTask;
    }

    private void RelogDb(HttpContext context, string role)
    {
        var db = context.RequestServices.GetRequiredService<IDbConnection>();
        if (db.State != ConnectionState.Open) db.Open();
        var command = db.CreateCommand();
        command.CommandText = $"SET ROLE {role};";
        command.ExecuteNonQuery();
        command.Dispose();
    }
    
    private void ResetRole(HttpContext context)
    {
        var db = context.RequestServices.GetRequiredService<IDbConnection>();
        if (db.State != ConnectionState.Open) db.Open();
        var command = db.CreateCommand();
        command.CommandText = $"RESET ROLE;";
        command.ExecuteNonQuery();
        command.Dispose();
    }
}

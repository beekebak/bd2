namespace bd2.Web.Auth;

public static class AuthHelper
{
    public static void SetAuthCookie(HttpContext context, string login, string hashedPassword, string role)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        var value = $"{login}|{hashedPassword}|{role}";
        context.Response.Cookies.Append("auth", value, cookieOptions);
    }

    public static (string login, string hashedPassword, string role)? GetAuthCookie(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("auth", out var value))
        {
            var parts = value.Split('|');
            if (parts.Length == 3)
            {
                return (parts[0], parts[1], parts[2]);
            }
        }
        return null;
    }

    public static void ClearAuthCookie(HttpContext context)
    {
        context.Response.Cookies.Delete("auth");
    }
}
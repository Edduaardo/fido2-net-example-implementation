using System.Security.Claims;
using Fido2Authentication.Business.Interfaces;
using Fido2Authentication.Business.Interfaces.Services;
using Fido2Authentication.Business.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Fido2Authentication.Infrastructure;

public class TokenService(
    IUserService userService,
    IHttpContextAccessor httpContext) : ITokenService
{
    private readonly IUserService _userService = userService;
    private readonly IHttpContextAccessor _httpContext = httpContext;

    public async Task LoginUserAsync(User user)
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, user.Email),
            new ("FullName", user.Name),
            //new (ClaimTypes.Role, "Administrator"),
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            //AllowRefresh = <bool>,
            // Refreshing the authentication session should be allowed.

            //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
            // The time at which the authentication ticket expires. A 
            // value set here overrides the ExpireTimeSpan option of 
            // CookieAuthenticationOptions set with AddCookie.

            //IsPersistent = true,
            // Whether the authentication session is persisted across 
            // multiple requests. When used with cookies, controls
            // whether the cookie's lifetime is absolute (matching the
            // lifetime of the authentication ticket) or session-based.

            //IssuedUtc = <DateTimeOffset>,
            // The time at which the authentication ticket was issued.

            //RedirectUri = <string>
            // The full path or absolute URI to be used as an http 
            // redirect response value.
        };

        await _httpContext.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimsIdentity), 
            authProperties
        );
    }
}

using System.Net;
using System.Text;
using Fido2Authentication.Business.Interfaces;
using Fido2Authentication.Business.Interfaces.Services;
using Fido2Authentication.Web.Models;
using Fido2NetLib;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fido2Authentication.Web.Controllers;

[AllowAnonymous]
public class LoginController(
    IUserService userService,
    ITokenService tokenService) : Controller
{
    private readonly IUserService _userService = userService;
    private readonly ITokenService _tokenService = tokenService;

    [HttpGet]
    public IActionResult Index(string returnUrl)
    {
        if (User?.Identity?.IsAuthenticated ?? false)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            }
        );
    }

    [HttpPost]
    public async Task<IActionResult> LoginAsync(
        LoginViewModel loginViewModel,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetByEmailAsync(loginViewModel.Login, cancellationToken);

        if (_userService.VerifyLogin(user!, loginViewModel.Password))
        {
            await _tokenService.LoginUserAsync(user);

            if (!string.IsNullOrEmpty(loginViewModel.ReturnUrl))
                return Redirect(loginViewModel.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpGet("get-passkey-assertion-options")]
    public async Task<IActionResult> GetPasskeyAssertionOptionsAsync(
        [FromQuery] string login,
        [FromQuery] string userVerification,
        CancellationToken cancellationToken)
    {
        try
        {
            return Json(
                await _userService.GetPasskeyAssertionOptionsAsync(
                    login,
                    null,
                    cancellationToken
                )
            );
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    [HttpPost("make-passkey-assertion")]
    public async Task<IActionResult> MakePasskeyAssertionAsync(
        [FromBody] AuthenticatorAssertionRawResponse authenticatorAssertionRawResponse,
        CancellationToken cancellationToken)
    {
        try
        {
            await _userService.MakePasskeyAssertionAsync(
                authenticatorAssertionRawResponse,
                cancellationToken
            );
           
            var user = await _userService.GetByEmailAsync(
                Encoding.UTF8.GetString(authenticatorAssertionRawResponse.Response.UserHandle),
                cancellationToken
            );

            await _tokenService.LoginUserAsync(user);

            return Json(new { Message = "Success.", Status = HttpStatusCode.OK });
        }
        catch (Fido2VerificationException fido2VerificationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Login");
    }
}

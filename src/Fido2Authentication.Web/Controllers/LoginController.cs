using System.Net;
using System.Text;
using Fido2Authentication.Business.Interfaces;
using Fido2Authentication.Business.Interfaces.Services;
using Fido2Authentication.Web.ExtensionMethods;
using Fido2Authentication.Web.Models;
using Fido2Authentication.Web.ResponseViewModels;
using Fido2NetLib;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fido2Authentication.Web.Controllers;

[AllowAnonymous]
public class LoginController(
    ILogger<LoginController> logger,
    IUserService userService,
    ITokenService tokenService) : Controller
{
    private readonly ILogger<LoginController> _logger = logger;
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
        try
        {
            var user = await _userService.GetByEmailAsync(loginViewModel.Login, cancellationToken);

            if (user is null)
                return RedirectToAction("Index")
                    .ErrorMessage("Username or password may be wrong, try again.");

            if (_userService.VerifyLogin(user, loginViewModel.Password))
            {
                await _tokenService.LoginUserAsync(user!);

                if (!string.IsNullOrEmpty(loginViewModel.ReturnUrl))
                    return Redirect(loginViewModel.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index")
                .ErrorMessage("Username or password may be wrong, try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoginAsync()");

            return RedirectToAction("Index")
                .ErrorMessage("An error ocurred, try again later.");
        }
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
            _logger.LogError(ex, "GetPasskeyAssertionOptionsAsync()");
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponseViewModel
                {
                    Message = ""
                }
            );
        }
    }

    [HttpPost("make-passkey-assertion")]
    public async Task<IActionResult> MakePasskeyAssertionAsync(
        [FromBody] AuthenticatorAssertionRawResponse authenticatorAssertionRawResponse,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetUserByPasskeyCredentialIdAsync(
                authenticatorAssertionRawResponse.RawId,
                cancellationToken
            );

            if (user is null)
                return BadRequest();

            await _userService.MakePasskeyAssertionAsync(
                authenticatorAssertionRawResponse,
                cancellationToken
            );

            await _tokenService.LoginUserAsync(user);

            return Json(new { Message = "Success.", Status = HttpStatusCode.OK });
        }
        catch (Fido2VerificationException fido2VerificationException)
        {
            _logger.LogError(fido2VerificationException, "MakePasskeyAssertionAsync()");
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponseViewModel
                {
                    Message = ""
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MakePasskeyAssertionAsync()");
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponseViewModel
                {
                    Message = ""
                }
            );
        }
    }

    [HttpGet]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Login");
    }
}

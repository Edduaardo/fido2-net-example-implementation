using Fido2Authentication.Business.Interfaces.Services;
using Fido2Authentication.Web.Models;
using Fido2Authentication.Web.ResponseViewModels;
using Fido2NetLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fido2Authentication.Web.Controllers;

[Authorize]
public class ProfileController(IUserService userService) : Controller
{
    private readonly IUserService _userService = userService;

    public async Task<IActionResult> Index() => View(await _userService.GetByEmailAsync(User.Identity!.Name!));

    public IActionResult Security() => View();

    [HttpPost]
    public async Task<IActionResult> ChangePasswordAsync(
        ChangePasswordViewModel changePasswordViewModel,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid) return RedirectToAction("Security", changePasswordViewModel);

            var user = await _userService.GetByEmailAsync(HttpContext.User.Identity!.Name!, cancellationToken);

            if (!_userService.VerifyLogin(user, changePasswordViewModel.CurrentPassword))
                return View();

            await _userService.ChangePasswordAsync(user, changePasswordViewModel.NewPassword, cancellationToken);

            return View();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    [HttpGet("passkey-get-attestation-options")]
    public async Task<IActionResult> PasskeyGetAttestationOptionsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return Json(
                await _userService.PasskeyGetAttestationOptionsAsync(
                    HttpContext.User.Identity.Name,
                    cancellationToken
                )
            );
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    [HttpPost("save-passkey/{passkeyName}")]
    public async Task<IActionResult> SavePasskeyAsync(
        [FromBody] AuthenticatorAttestationRawResponse authenticatorAttestationRawResponse,
        [FromRoute] string passkeyName,
        CancellationToken cancellationToken)
    {
        try
        {
            await _userService.SavePasskeyAsync(
                authenticatorAttestationRawResponse,
                passkeyName,
                cancellationToken
            );

            return Ok();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    [HttpGet("get-user-passkeys")]
    public async Task<IActionResult> GetUserPasskeysAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetByEmailAsync(User.Identity!.Name!, cancellationToken);

            return Json(new
            {
                data = user.Passkeys.Select(x => new GetUserPasskeysResponseViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreationDate = x.CreationDate
                })
            });
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    [HttpDelete("delete-user-passkey")]
    public async Task<IActionResult> DeleteUserPasskeyAsync(
        Guid passkeyId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _userService.DeleteUserPasskeyAsync(passkeyId, cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}

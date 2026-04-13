using System.Net;
using Fido2Authentication.Business.Interfaces.Services;
using Fido2Authentication.Web.ExtensionMethods;
using Fido2Authentication.Web.Models;
using Fido2Authentication.Web.ResponseViewModels;
using Fido2NetLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fido2Authentication.Web.Controllers;

[Authorize]
public class ProfileController(
    ILogger<ProfileController> logger,
    IUserService userService) : Controller
{
    private readonly ILogger<ProfileController> _logger = logger;
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

            if (!_userService.VerifyLogin(user!, changePasswordViewModel.CurrentPassword))
                return View();

            await _userService.ChangePasswordAsync(user!, changePasswordViewModel.NewPassword, cancellationToken);

            return RedirectToAction("Security")
                .SuccessMessage("Passoword changed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangePasswordAsync()");

            return RedirectToAction("Security")
                .ErrorMessage("An error ocurred, try again later.");
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
                    HttpContext.User.Identity!.Name!,
                    cancellationToken
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PasskeyGetAttestationOptionsAsync()");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponseViewModel
                {
                    Message = ""
                }
            );
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

            return Json(new { Message = "Success.", Status = HttpStatusCode.OK });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SavePasskeyAsync()");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponseViewModel
                {
                    Message = ""
                }
            );
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
                data = user!.Passkeys.Select(x => new GetUserPasskeysResponseViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreationDate = x.CreationDate
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserPasskeysAsync()");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponseViewModel
                {
                    Message = ""
                }
            );
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
            _logger.LogError(ex, "DeleteUserPasskeyAsync()");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponseViewModel
                {
                    Message = ""
                }
            );
        }
    }

    [HttpPut("edit-passkey-name")]
    public async Task<IActionResult> EditPasskeyNameAsync(
        [FromBody] ChangePasskeyNameViewModel changePasskeyNameViewModel,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetUserByPasskeyIdAsync(
                changePasskeyNameViewModel.Id,
                cancellationToken);

            user.Passkeys.FirstOrDefault(x => x.Id == changePasskeyNameViewModel.Id)?.Name = changePasskeyNameViewModel.Name;

            await _userService.UpdateAsync(user);

            return Json(new { });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteUserPasskeyAsync()");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponseViewModel
                {
                    Message = ""
                }
            );
        }
    }
}
